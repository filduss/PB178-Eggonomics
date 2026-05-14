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

public partial class FridgeOverviewViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFridgeOperationService _fridgeOperationService;
    private readonly AppDbContext _db;
    private readonly Supabase.Client _supabaseClient;

    private Fridge? _fridge;
    private bool _sortAscending = false;

    [ObservableProperty]
    private string fridgeName = string.Empty;

    [ObservableProperty]
    private string sortLabel = "↓ Net Value";

    [ObservableProperty]
    private bool isDebtsVisible = false;

    [ObservableProperty]
    private bool isOwner = false;

    [ObservableProperty]
    private bool hasDebts = false;

    [ObservableProperty]
    private bool hasNoDebts = true;

    public ObservableCollection<UserFridgeStats> Members { get; } = new();
    public ObservableCollection<DebtResult> Debts { get; } = new();

    public FridgeOverviewViewModel(
        IServiceProvider serviceProvider,
        IFridgeOperationService fridgeOperationService,
        AppDbContext db,
        Supabase.Client supabaseClient)
    {
        _serviceProvider = serviceProvider;
        _fridgeOperationService = fridgeOperationService;
        _db = db;
        _supabaseClient = supabaseClient;
    }

    public async Task Initialize(Fridge fridge)
    {
        _fridge = fridge;
        FridgeName = fridge.Name;

        var email = _supabaseClient.Auth.CurrentUser?.Email;
        if (email != null)
        {
            var currentUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            IsOwner = currentUser != null && fridge.OwnerId == currentUser.Id;
        }
    }

    [RelayCommand]
    public async Task LoadStats()
    {
        if (_fridge == null)
        {
            return;
        }

        var stats = await _fridgeOperationService.GetUserStatsAsync(_fridge.Id);
        ApplySort(stats);
    }

    [RelayCommand]
    public void ToggleSort()
    {
        _sortAscending = !_sortAscending;
        SortLabel = _sortAscending ? "↑ Net Value" : "↓ Net Value";
        ApplySort(Members.ToList());
    }

    [RelayCommand]
    public async Task CalculateDebts()
    {
        if (_fridge == null)
        {
            return;
        }

        var debts = await _fridgeOperationService.CalculateDebtsAsync(_fridge.Id);

        Debts.Clear();
        foreach (var debt in debts)
        {
            Debts.Add(debt);
        }

        HasDebts = debts.Count > 0;
        HasNoDebts = !HasDebts;
        IsDebtsVisible = true;
    }

    [RelayCommand]
    public void CloseDebts()
    {
        IsDebtsVisible = false;
    }

    [RelayCommand]
    public async Task SettleDebts()
    {
        if (_fridge == null)
        {
            return;
        }

        bool confirmed = await Application.Current!.Windows[0].Page!
            .DisplayAlert("Settle Debts", "This will permanently delete all operation history for this fridge. Continue?", "Yes, settle", "Cancel");

        if (!confirmed)
        {
            return;
        }

        await _fridgeOperationService.SettleDebtsAsync(_fridge.Id);

        Debts.Clear();
        Members.Clear();
        HasDebts = false;
        HasNoDebts = true;
        IsDebtsVisible = false;
    }

    private void ApplySort(List<UserFridgeStats> stats)
    {
        var sorted = _sortAscending
            ? stats.OrderBy(s => s.NetValue)
            : stats.OrderByDescending(s => s.NetValue);

        Members.Clear();
        foreach (var stat in sorted)
        {
            Members.Add(stat);
        }
    }

    [RelayCommand]
    public void GoBack()
    {
        var fridgePage = _serviceProvider.GetRequiredService<FridgePage>();
        if (fridgePage.BindingContext is FridgeViewModel vm && _fridge != null)
        {
            vm.Initialize(_fridge);
        }
        Application.Current!.Windows[0].Page = fridgePage;
    }
}
