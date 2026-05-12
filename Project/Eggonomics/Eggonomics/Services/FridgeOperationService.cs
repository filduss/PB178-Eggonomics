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

        public async Task RecordAsync(OperationType type, int fridgeId, int quantity)
        {
            var email = _supabaseClient.Auth.CurrentUser!.Email;
            var user = await _db.Users.FirstAsync(u => u.Email == email);
            var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);
            var fridgeUser = await _db.FridgeUsers.FirstAsync(fu => fu.UserId == user.Id && fu.FridgeId == fridgeId);

            _db.EggOperations.Add(new EggOperation(type, fridge, fridgeUser, quantity));
            await _db.SaveChangesAsync();
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
    }
}
