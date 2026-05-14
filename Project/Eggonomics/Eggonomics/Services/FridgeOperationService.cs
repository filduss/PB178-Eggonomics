using Eggonomics.Data;
using Eggonomics.Enums;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Microsoft.EntityFrameworkCore;

namespace Eggonomics.Services
{
    public class FridgeOperationService : IFridgeOperationService
    {
        private readonly AppDbContext _db;
        private readonly Supabase.Client _supabaseClient;

        public FridgeOperationService(AppDbContext db, Supabase.Client supabaseClient)
        {
            _db = db;
            _supabaseClient = supabaseClient;
        }

        public async Task RecordAsync(OperationType type, int fridgeId, int quantity, decimal monetaryValue = 0)
        {
            var email = _supabaseClient.Auth.CurrentUser!.Email;
            var user = await _db.Users.FirstAsync(u => u.Email == email);
            var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);
            var fridgeUser = await _db.FridgeUsers.FirstAsync(fu => fu.UserId == user.Id && fu.FridgeId == fridgeId);

            _db.EggOperations.Add(new EggOperation(type, fridge, fridgeUser, quantity, monetaryValue));
            await _db.SaveChangesAsync();
        }

        public async Task RecordTakeAsync(int fridgeId, List<(int trayId, int quantity, decimal pricePerEgg)> items)
        {
            var email = _supabaseClient.Auth.CurrentUser!.Email;
            var user = await _db.Users.FirstAsync(u => u.Email == email);
            var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);
            var fridgeUser = await _db.FridgeUsers.FirstAsync(fu => fu.UserId == user.Id && fu.FridgeId == fridgeId);

            int totalQuantity = items.Sum(i => i.quantity);
            decimal totalMonetary = items.Sum(i => i.quantity * i.pricePerEgg);

            var operation = new EggOperation(OperationType.Withdraw, fridge, fridgeUser, totalQuantity, totalMonetary);
            _db.EggOperations.Add(operation);
            await _db.SaveChangesAsync();

            foreach (var (trayId, qty, _) in items)
            {
                _db.EggOperationBatchLinks.Add(new EggOperationBatchLink
                {
                    EggOperationId = operation.Id,
                    EggTrayId = trayId,
                    QuantityTaken = qty
                });
            }
            await _db.SaveChangesAsync();
        }

        public async Task<int> ProcessExpiredTraysAsync(int fridgeId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);

            var expiredTrays = await _db.EggTrays
                .Where(t => t.FridgeId == fridgeId && t.ExpirationDate < today && t.QuantityRemaining > 0)
                .Include(t => t.AddedBy)
                .ToListAsync();

            if (expiredTrays.Count == 0)
            {
                return 0;
            }

            int totalExpiredEggs = 0;

            foreach (var tray in expiredTrays)
            {
                var fridgeUser = await _db.FridgeUsers
                    .FirstOrDefaultAsync(fu => fu.UserId == tray.AddedByUserId && fu.FridgeId == fridgeId);

                if (fridgeUser == null)
                {
                    continue;
                }

                decimal monetaryValue = tray.QuantityRemaining * tray.PricePerEgg;
                _db.EggOperations.Add(new EggOperation(OperationType.Expire, fridge, fridgeUser, tray.QuantityRemaining, monetaryValue));
                totalExpiredEggs += tray.QuantityRemaining;
                _db.EggTrays.Remove(tray);
            }

            await _db.SaveChangesAsync();
            return totalExpiredEggs;
        }

        public async Task<List<EggOperation>> GetByFridgeAsync(int fridgeId)
        {
            return await _db.EggOperations
                .Where(o => o.FridgeId == fridgeId)
                .Include(o => o.FridgeUser)
                .ThenInclude(fu => fu.User)
                .OrderByDescending(o => o.PerformedAt)
                .ToListAsync();
        }

        public async Task<List<UserFridgeStats>> GetUserStatsAsync(int fridgeId)
        {
            var operations = await _db.EggOperations
                .Where(o => o.FridgeId == fridgeId)
                .Include(o => o.FridgeUser)
                    .ThenInclude(fu => fu.User)
                .ToListAsync();

            return operations
                .GroupBy(o => o.FridgeUser.User.DisplayName)
                .Select(g => new UserFridgeStats
                {
                    DisplayName  = g.Key,
                    EggsGiven    = g.Where(o => o.Type == OperationType.Insert).Sum(o => o.TotalQuantity),
                    EggsTaken    = g.Where(o => o.Type == OperationType.Withdraw).Sum(o => o.TotalQuantity),
                    EggsExpired  = g.Where(o => o.Type == OperationType.Expire).Sum(o => o.TotalQuantity),
                    ValueGiven   = g.Where(o => o.Type == OperationType.Insert).Sum(o => o.MonetaryValue),
                    ValueTaken   = g.Where(o => o.Type == OperationType.Withdraw).Sum(o => o.MonetaryValue),
                    ValueExpired = g.Where(o => o.Type == OperationType.Expire).Sum(o => o.MonetaryValue),
                })
                .ToList();
        }

        public async Task<List<DebtResult>> CalculateDebtsAsync(int fridgeId)
        {
            var operations = await _db.EggOperations
                .Where(o => o.FridgeId == fridgeId)
                .Include(o => o.FridgeUser)
                    .ThenInclude(fu => fu.User)
                .ToListAsync();

            var activeTrays = await _db.EggTrays
                .Where(t => t.FridgeId == fridgeId && t.QuantityRemaining > 0)
                .Include(t => t.AddedBy)
                .ToListAsync();

            var remainingByUser = activeTrays
                .GroupBy(t => t.AddedBy.DisplayName)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.QuantityRemaining * t.PricePerEgg));

            var balances = GetEffectiveBalances(operations, remainingByUser);
            return MinimizeTransactions(balances);
        }

        private static Dictionary<string, decimal> GetEffectiveBalances(
            List<EggOperation> operations, Dictionary<string, decimal> remainingByUser)
        {
            var balances = new Dictionary<string, decimal>();

            foreach (var group in operations.GroupBy(o => o.FridgeUser.User.DisplayName))
            {
                string name = group.Key;
                decimal given = group.Where(o => o.Type == OperationType.Insert).Sum(o => o.MonetaryValue);
                decimal taken = group.Where(o => o.Type == OperationType.Withdraw).Sum(o => o.MonetaryValue);
                decimal expired = group.Where(o => o.Type == OperationType.Expire).Sum(o => o.MonetaryValue);
                remainingByUser.TryGetValue(name, out decimal remaining);

                decimal balance = given - remaining - taken - expired;
                if (balance != 0)
                {
                    balances[name] = balance;
                }
            }

            return balances;
        }

        private static List<DebtResult> MinimizeTransactions(Dictionary<string, decimal> balances)
        {
            var creditors = balances.Where(b => b.Value > 0).Select(b => (Name: b.Key, Balance: b.Value)).ToList();
            var debtors   = balances.Where(b => b.Value < 0).Select(b => (Name: b.Key, Balance: b.Value)).ToList();
            var results   = new List<DebtResult>();

            while (debtors.Count > 0 && creditors.Count > 0)
            {
                debtors.Sort((a, b) => a.Balance.CompareTo(b.Balance));
                creditors.Sort((a, b) => b.Balance.CompareTo(a.Balance));

                var debtor   = debtors[0];
                var creditor = creditors[0];
                decimal payment = Math.Round(Math.Min(Math.Abs(debtor.Balance), creditor.Balance), 2);

                results.Add(new DebtResult
                {
                    FromDisplayName = debtor.Name,
                    ToDisplayName   = creditor.Name,
                    Amount          = payment
                });

                debtors[0] = (debtor.Name, debtor.Balance + payment);
                creditors[0] = (creditor.Name,creditor.Balance - payment);

                if (Math.Abs(debtors[0].Balance) < 0.01m)
                {
                    debtors.RemoveAt(0);
                }

                if (Math.Abs(creditors[0].Balance) < 0.01m)
                {
                    creditors.RemoveAt(0);
                }
            }

            return results;
        }

        public async Task SettleDebtsAsync(int fridgeId)
        {
            var remainingTrays = await _db.EggTrays
                .Where(t => t.FridgeId == fridgeId && t.QuantityRemaining > 0)
                .ToListAsync();

            var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);

            var operations = await _db.EggOperations
                .Where(o => o.FridgeId == fridgeId)
                .ToListAsync();

            _db.EggOperations.RemoveRange(operations);
            await _db.SaveChangesAsync();

            foreach (var tray in remainingTrays)
            {
                var fridgeUser = await _db.FridgeUsers.FirstOrDefaultAsync(fu => fu.UserId == tray.AddedByUserId && fu.FridgeId == fridgeId);

                if (fridgeUser == null)
                {
                    continue;
                }

                decimal monetaryValue = tray.QuantityRemaining * tray.PricePerEgg;
                _db.EggOperations.Add(new EggOperation(OperationType.Insert, fridge, fridgeUser, tray.QuantityRemaining, monetaryValue));
            }

            await _db.SaveChangesAsync();
        }
    }
}
