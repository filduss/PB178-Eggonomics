using Eggonomics.ViewModels;

namespace Eggonomics.Views;

public partial class FridgePage : ContentPage
{
    public FridgePage(FridgeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as FridgeViewModel;
        vm?.LoadTraysCommand.Execute(null);
        vm?.LoadOperationsCommand.Execute(null);
    }
}
