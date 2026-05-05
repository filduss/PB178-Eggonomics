using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Models;
using Eggonomics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Eggonomics.ViewModels;

public partial class FridgeViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string fridgeName;

    public FridgeViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize(Fridge fridge)
    {
        FridgeName = fridge.Name;
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
        // TODO
    }
}
