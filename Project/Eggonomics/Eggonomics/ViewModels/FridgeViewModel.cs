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

        var tray = await _eggTrayService.InsertAsync(
            name: Name,
            price: Price,
            quantityAdded: QuantityAdded,
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
    public void Insert()
    {
        // TODO preped 4 operations
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

        var sorted = EggTrays.OrderBy(t => t.ExpirationDate).ToList();
        int remaining = TakeQuantity;

        foreach (var tray in sorted)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (tray.QuantityRemaining <= remaining)
            {
                remaining -= tray.QuantityRemaining;
                tray.QuantityRemaining = 0;
                await _eggTrayService.DeleteAsync(tray);
                EggTrays.Remove(tray);
            }
            else
            {
                tray.QuantityRemaining -= remaining;
                remaining = 0;
                await _eggTrayService.UpdateAsync(tray);
            }
        }

        IsTakeEggsVisible = false;
    }
}
