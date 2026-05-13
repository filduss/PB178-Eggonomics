using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Enums;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Eggonomics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Eggonomics.ViewModels;

public partial class FridgeViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEggTrayService _eggTrayService;
    private readonly IFridgeOperationService _fridgeOperationService;

    private Fridge? _fridge;

    [ObservableProperty]
    private string fridgeName;

    [ObservableProperty]
    private bool isTraySelectVisible = false;

    [ObservableProperty] private string name;
    [ObservableProperty] private decimal price;
    [ObservableProperty] private string notes;
    [ObservableProperty] private Suppliers origin;
    [ObservableProperty] private int quantityAdded;
    [ObservableProperty] private DateTime expirationDate = DateTime.Today;

    public List<Suppliers> Origins { get; } = Enum.GetValues<Suppliers>().ToList();
    public ObservableCollection<EggTray> EggTrays { get; } = new();
    public ObservableCollection<EggOperation> Operations { get; } = new();

    public int TotalEggs
    {
        get { return EggTrays.Sum(t => t.QuantityRemaining); }
    }

    [ObservableProperty] private bool hasError = false;
    [ObservableProperty] private string errorMessage;

    [ObservableProperty] private bool isTakeEggsVisible = false;
    [ObservableProperty] private int takeQuantity;
    [ObservableProperty] private bool hasTakeError = false;
    [ObservableProperty] private string takeErrorMessage;

    public FridgeViewModel(IServiceProvider serviceProvider, IEggTrayService eggTrayService, IFridgeOperationService fridgeOperationService)
    {
        _serviceProvider = serviceProvider;
        _eggTrayService = eggTrayService;
        _fridgeOperationService = fridgeOperationService;

        EggTrays.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalEggs));
    }

    public void Initialize(Fridge fridge)
    {
        _fridge = fridge;
        FridgeName = fridge.Name;
    }

    public async Task<int> CheckExpiredTraysAsync()
    {
        if (_fridge == null)
        {
            return 0;
        }
        return await _fridgeOperationService.ProcessExpiredTraysAsync(_fridge.Id);
    }

    [RelayCommand]
    public async Task LoadTrays()
    {
        if (_fridge == null)
        {
            return;
        }

        var trays = await _eggTrayService.GetByFridgeAsync(_fridge.Id);
        EggTrays.Clear();
        foreach (var tray in trays)
        {
            EggTrays.Add(tray);
        }
    }

    [RelayCommand]
    public async Task LoadOperations()
    {
        if (_fridge == null)
        {
            return;
        }

        var ops = await _fridgeOperationService.GetByFridgeAsync(_fridge.Id);
        Operations.Clear();
        foreach (var op in ops)
        {
            Operations.Add(op);
        }
    }

    [RelayCommand]
    public async Task AddTray()
    {
        if (_fridge == null || string.IsNullOrWhiteSpace(Name) || QuantityAdded <= 0)
        {
            ErrorMessage = "Please fill in all required fields.";
            HasError = true;
            return;
        }

        HasError = false;
        ErrorMessage = null;

        int qty = QuantityAdded;

        var tray = await _eggTrayService.InsertAsync(
            name: Name,
            price: Price,
            quantityAdded: qty,
            origin: Origin,
            expirationDate: DateOnly.FromDateTime(ExpirationDate),
            notes: Notes,
            fridgeId: _fridge.Id
        );

        EggTrays.Insert(0, tray);
        Name = string.Empty;
        Price = 0;
        QuantityAdded = 0;
        Notes = string.Empty;
        ExpirationDate = DateTime.Today;
        IsTraySelectVisible = false;

        await _fridgeOperationService.RecordAsync(OperationType.Insert, _fridge.Id, qty, tray.Price);
        await LoadOperations();
    }

    [RelayCommand]
    public void OpenTraySelect()
    {
        IsTraySelectVisible = true;
    }

    [RelayCommand]
    public void CloseTraySelect()
    {
        IsTraySelectVisible = false;
    }

    [RelayCommand]
    public void GoBack()
    {
        var fridgesPage = _serviceProvider.GetRequiredService<FridgesPage>();
        Application.Current!.Windows[0].Page = fridgesPage;
    }

    [RelayCommand]
    public void GoToOverview()
    {
        var overviewPage = _serviceProvider.GetRequiredService<FridgeOverviewPage>();
        if (overviewPage.BindingContext is FridgeOverviewViewModel vm && _fridge != null)
        {
            vm.Initialize(_fridge);
        }
        Application.Current!.Windows[0].Page = overviewPage;
    }

    [RelayCommand]
    public void OpenTakeEggs()
    {
        TakeQuantity = 0;
        HasTakeError = false;
        TakeErrorMessage = null;
        IsTakeEggsVisible = true;
    }

    [RelayCommand]
    public void CloseTakeEggs()
    {
        IsTakeEggsVisible = false;
    }

    [RelayCommand]
    public async Task TakeEggs()
    {
        if (TakeQuantity <= 0)
        {
            TakeErrorMessage = "Enter a valid amount.";
            HasTakeError = true;
            return;
        }

        int totalAvailable = EggTrays.Sum(t => t.QuantityRemaining);
        if (TakeQuantity > totalAvailable)
        {
            TakeErrorMessage = $"Only {totalAvailable} eggs available.";
            HasTakeError = true;
            return;
        }

        HasTakeError = false;
        TakeErrorMessage = null;

        int qty = TakeQuantity;
        var sorted = EggTrays.OrderBy(t => t.ExpirationDate).ToList();
        int remaining = qty;
        var batchItems = new List<(int trayId, int quantity, decimal pricePerEgg)>();

        foreach (var tray in sorted)
        {
            if (remaining <= 0)
            {
                break;
            }
            int take = Math.Min(tray.QuantityRemaining, remaining);
            batchItems.Add((tray.Id, take, tray.PricePerEgg));
            remaining -= take;
        }

        await _fridgeOperationService.RecordTakeAsync(_fridge!.Id, batchItems);

        foreach (var (trayId, take, _) in batchItems)
        {
            var tray = EggTrays.First(t => t.Id == trayId);
            if (tray.QuantityRemaining <= take)
            {
                tray.QuantityRemaining = 0;
                await _eggTrayService.DeleteAsync(tray);
                EggTrays.Remove(tray);
            }
            else
            {
                tray.QuantityRemaining -= take;
                await _eggTrayService.UpdateAsync(tray);
            }
        }

        OnPropertyChanged(nameof(TotalEggs));
        await LoadOperations();
        IsTakeEggsVisible = false;
    }
}
