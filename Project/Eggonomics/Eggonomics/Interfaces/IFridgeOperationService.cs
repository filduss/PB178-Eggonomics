using Eggonomics.Enums;
using Eggonomics.Models;

namespace Eggonomics.Interfaces
{
    public interface IFridgeOperationService
    {
        Task RecordAsync(OperationType type, int fridgeId, int quantity);
        Task<List<EggOperation>> GetByFridgeAsync(int fridgeId);
    }
}
