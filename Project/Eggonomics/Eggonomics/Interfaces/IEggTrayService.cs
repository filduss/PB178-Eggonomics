using Eggonomics.Enums;
using Eggonomics.Models;

namespace Eggonomics.Interfaces;

public interface IEggTrayService
{
    Task<EggTray> InsertAsync(string name, decimal price, int quantityAdded, Suppliers origin, DateOnly expirationDate, string? notes, int fridgeId);
    Task<List<EggTray>> GetByFridgeAsync(int fridgeId);
    Task UpdateAsync(EggTray tray);
    Task DeleteAsync(EggTray tray);
}
