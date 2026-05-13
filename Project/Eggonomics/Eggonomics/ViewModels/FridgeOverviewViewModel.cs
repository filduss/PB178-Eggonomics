using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Eggonomics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Eggonomics.ViewModels;

public partial class FridgeOverviewViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFridgeOperationService _fridgeOperationService;

    private Fridge? _fridge;
    private bool _sortAscending = false;

    [ObservableProperty]
    private string fridgeName = string.Empty;

    [ObservableProperty]
    private string sortLabel = "↓ Net Value";

    public ObservableCollection<UserFridgeStats> Members { get; } = new();

    public FridgeOverviewViewModel(IServiceProvider serviceProvider, IFridgeOperationService fridgeOperationService)
    {
        _serviceProvider = serviceProvider;
        _fridgeOperationService = fridgeOperationService;
    }

    public void Initialize(Fridge fridge)
    {
        _fridge = fridge;
        FridgeName = fridge.Name;
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
