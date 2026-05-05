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
        (BindingContext as FridgeViewModel)?.LoadTraysCommand.Execute(null);
    }
}
