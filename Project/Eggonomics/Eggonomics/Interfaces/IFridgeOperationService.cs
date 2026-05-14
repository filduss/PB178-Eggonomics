using Eggonomics.Enums;
using Eggonomics.Models;

namespace Eggonomics.Interfaces
{
    public interface IFridgeOperationService
    {
        Task RecordAsync(OperationType type, int fridgeId, int quantity, decimal monetaryValue = 0);
        Task RecordTakeAsync(int fridgeId, List<(int trayId, int quantity, decimal pricePerEgg)> items);
        Task<int> ProcessExpiredTraysAsync(int fridgeId);
        Task<List<EggOperation>> GetByFridgeAsync(int fridgeId);
        Task<List<UserFridgeStats>> GetUserStatsAsync(int fridgeId);
        Task<List<DebtResult>> CalculateDebtsAsync(int fridgeId);
        Task SettleDebtsAsync(int fridgeId);
    }
}
