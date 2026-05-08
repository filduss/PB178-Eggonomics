using Eggonomics.Data;
using Eggonomics.Enums;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Microsoft.EntityFrameworkCore;

namespace Eggonomics.Services;

public class EggTrayService : IEggTrayService
{
    private readonly AppDbContext _db;
    private readonly Supabase.Client _supabaseClient;

    public EggTrayService(AppDbContext db, Supabase.Client supabaseClient)
    {
        _db = db;
        _supabaseClient = supabaseClient;
    }

    public async Task<EggTray> InsertAsync(string name, decimal price, int quantityAdded, Suppliers origin, DateOnly expirationDate, string? notes, int fridgeId)
    {
        var email = _supabaseClient.Auth.CurrentUser!.Email;
        var user = await _db.Users.FirstAsync(u => u.Email == email);
        var fridge = await _db.Fridges.FirstAsync(f => f.Id == fridgeId);

        var tray = new EggTray(
            quantityAdded: quantityAdded,
            name: name,
            origin: origin,
            expirationDate: expirationDate,
            price: price,
            pricePerEgg: quantityAdded > 0 ? price / quantityAdded : 0,
            fridge: fridge,
            addedBy: user,
            notes: notes
        );

        _db.EggTrays.Add(tray);
        await _db.SaveChangesAsync();

        return tray;
    }

    public async Task<List<EggTray>> GetByFridgeAsync(int fridgeId)
    {
        return await _db.EggTrays
            .Where(t => t.FridgeId == fridgeId)
            .OrderByDescending(t => t.AddedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(EggTray tray)
    {
        _db.EggTrays.Update(tray);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(EggTray tray)
    {
        _db.EggTrays.Remove(tray);
        await _db.SaveChangesAsync();
    }
}
