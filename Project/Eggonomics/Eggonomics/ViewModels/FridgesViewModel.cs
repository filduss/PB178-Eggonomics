using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Data;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Eggonomics.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Eggonomics.ViewModels;

public partial class FridgesViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _db;
    private readonly Supabase.Client _supabaseClient;

    private User? _currentUser;
    private Fridge? _managingFridge;

    [ObservableProperty] public partial bool IsCreateFridgeVisible { get; set; }
    [ObservableProperty] public partial string? NewFridgeName { get; set; }
    [ObservableProperty] public partial bool IsViewMembersVisible { get; set; }
    [ObservableProperty] public partial string? SelectedFridgeName { get; set; }
    [ObservableProperty] public partial bool IsManageVisible { get; set; }
    [ObservableProperty] public partial string? AddMemberEmail { get; set; }
    [ObservableProperty] public partial string? ManageError { get; set; }
    [ObservableProperty] public partial bool HasManageError { get; set; }

    public ObservableCollection<FridgeItem> Fridges { get; } = new();
    public ObservableCollection<User> FridgeMembers { get; } = new();

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
        if (email == null)
        {
            return;
        }

        _currentUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (_currentUser == null)
        {
            return;
        }

        var fridges = await _db.Fridges
            .Where(f => f.OwnerId == _currentUser.Id ||
                        f.Members.Any(fu => fu.UserId == _currentUser.Id && fu.IsActive))
            .ToListAsync();

        Fridges.Clear();
        foreach (var fridge in fridges)
        {
            Fridges.Add(new FridgeItem { Fridge = fridge, IsOwner = fridge.OwnerId == _currentUser.Id });
        }
    }

    [RelayCommand]
    public void EnterFridge(Fridge fridge)
    {
        var fridgePage = _serviceProvider.GetRequiredService<FridgePage>();
        if (fridgePage.BindingContext is FridgeViewModel vm)
        {
            vm.Initialize(fridge);
        }
        Application.Current!.Windows[0].Page = fridgePage;
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
        {
            return;
        }

        var fridge = new Fridge
        {
            Name = NewFridgeName,
            OwnerId = _currentUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.Fridges.Add(fridge);
        await _db.SaveChangesAsync();

        _db.FridgeUsers.Add(new FridgeUser
        {
            FridgeId = fridge.Id,
            UserId = _currentUser.Id,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _db.SaveChangesAsync();

        Fridges.Add(new FridgeItem { Fridge = fridge, IsOwner = true });
        NewFridgeName = string.Empty;
        IsCreateFridgeVisible = false;
    }

    [RelayCommand]
    public async Task ViewMembers(Fridge fridge)
    {
        SelectedFridgeName = fridge.Name;

        var members = await _db.FridgeUsers
            .Where(fu => fu.FridgeId == fridge.Id && fu.IsActive)
            .Include(fu => fu.User)
            .Select(fu => fu.User)
            .ToListAsync();

        FridgeMembers.Clear();
        foreach (var member in members)
        {
            FridgeMembers.Add(member);
        }

        IsViewMembersVisible = true;
    }

    [RelayCommand]
    public void CloseViewMembers() => IsViewMembersVisible = false;

    [RelayCommand]
    public async Task OpenManage(Fridge fridge)
    {
        _managingFridge = fridge;
        SelectedFridgeName = fridge.Name;
        AddMemberEmail = string.Empty;
        ManageError = null;
        HasManageError = false;

        var members = await _db.FridgeUsers
            .Where(fu => fu.FridgeId == fridge.Id && fu.IsActive && fu.UserId != fridge.OwnerId)
            .Include(fu => fu.User)
            .Select(fu => fu.User)
            .ToListAsync();

        FridgeMembers.Clear();
        foreach (var member in members)
        {
            FridgeMembers.Add(member);
        }

        IsManageVisible = true;
    }

    [RelayCommand]
    public void CloseManage()
    {
        _managingFridge = null;
        AddMemberEmail = string.Empty;
        ManageError = null;
        HasManageError = false;
        IsManageVisible = false;
    }

    [RelayCommand]
    public async Task AddMember()
    {
        if (string.IsNullOrWhiteSpace(AddMemberEmail) || _managingFridge == null)
        {
            return;
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == AddMemberEmail);
        if (user == null)
        {
            ManageError = "No user found with that email.";
            HasManageError = true;
            return;
        }

        var existing = await _db.FridgeUsers
            .FirstOrDefaultAsync(fu => fu.FridgeId == _managingFridge.Id && fu.UserId == user.Id);

        if (existing != null)
        {
            if (existing.IsActive)
            {
                ManageError = "User is already a member.";
                HasManageError = true;
                return;
            }

            // Re-activate a previously removed member instead of inserting a duplicate row.
            existing.IsActive = true;
            existing.JoinedAt = DateTime.UtcNow;
        }
        else
        {
            _db.FridgeUsers.Add(new FridgeUser
            {
                FridgeId = _managingFridge.Id,
                UserId = user.Id,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();

        FridgeMembers.Add(user);
        AddMemberEmail = string.Empty;
        ManageError = null;
        HasManageError = false;
    }

    [RelayCommand]
    public async Task RemoveMember(User user)
    {
        if (_managingFridge == null)
        {
            return;
        }

        var membership = await _db.FridgeUsers
            .FirstOrDefaultAsync(fu => fu.FridgeId == _managingFridge.Id && fu.UserId == user.Id);

        if (membership == null)
        {
            return;
        }

        // Soft-delete to preserve EggOperation history that references this FridgeUser row.
        membership.IsActive = false;
        await _db.SaveChangesAsync();

        FridgeMembers.Remove(user);
    }

    [RelayCommand]
    public async Task DeleteFridge(Fridge fridge)
    {
        // Delete in FK dependency order: operations → debt settlements → trays → members → fridge
        var operations = await _db.EggOperations.Where(o => o.FridgeId == fridge.Id).ToListAsync();
        _db.EggOperations.RemoveRange(operations);

        var settlements = await _db.DebtSettlements.Where(d => d.FridgeId == fridge.Id).ToListAsync();
        _db.DebtSettlements.RemoveRange(settlements);

        var trays = await _db.EggTrays.Where(t => t.FridgeId == fridge.Id).ToListAsync();
        _db.EggTrays.RemoveRange(trays);

        var members = await _db.FridgeUsers.Where(fu => fu.FridgeId == fridge.Id).ToListAsync();
        _db.FridgeUsers.RemoveRange(members);

        _db.Fridges.Remove(fridge);
        await _db.SaveChangesAsync();

        var item = Fridges.FirstOrDefault(fi => fi.Fridge.Id == fridge.Id);
        if (item != null)
        {
            Fridges.Remove(item);
        }
    }


}
