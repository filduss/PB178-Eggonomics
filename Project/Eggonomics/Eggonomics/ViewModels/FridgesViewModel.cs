using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Data;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Eggonomics.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Eggonomics.ViewModels;

public partial class FridgesViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _db;
    private readonly Supabase.Client _supabaseClient;

    private User? _currentUser;

    [ObservableProperty]
    private bool isCreateFridgeVisible = false;

    [ObservableProperty]
    private string newFridgeName;

    public ObservableCollection<Fridge> Fridges { get; } = new();

    public FridgesViewModel(IAuthService authService, IServiceProvider serviceProvider, AppDbContext db, Supabase.Client supabaseClient)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
        _db = db;
        _supabaseClient = supabaseClient;
    }

    [RelayCommand]
    public async Task LoadFridges()
    {
        var email = _supabaseClient.Auth.CurrentUser?.Email;
        if (email == null) return;

        _currentUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (_currentUser == null) return;

        var fridges = await _db.Fridges
            .Where(f => f.OwnerId == _currentUser.Id)
            .ToListAsync();

        Fridges.Clear();
        foreach (var fridge in fridges)
            Fridges.Add(fridge);
    }

    [RelayCommand]
    public async Task SignOut()
    {
        await _authService.SignOutAsync();
        var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        Application.Current!.Windows[0].Page = loginPage;
    }

    [RelayCommand]
    public void OpenCreateFridge() => IsCreateFridgeVisible = true;

    [RelayCommand]
    public void CloseCreateFridge()
    {
        NewFridgeName = string.Empty;
        IsCreateFridgeVisible = false;
    }

    [RelayCommand]
    public async Task ConfirmCreateFridge()
    {
        if (string.IsNullOrWhiteSpace(NewFridgeName) || _currentUser == null)
            return;

        var fridge = new Fridge
        {
            Name = NewFridgeName,
            OwnerId = _currentUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.Fridges.Add(fridge);
        await _db.SaveChangesAsync();

        Fridges.Add(fridge);
        NewFridgeName = string.Empty;
        IsCreateFridgeVisible = false;
    }
}
