using Eggonomics.ViewModels;

namespace Eggonomics.Views;

public partial class FridgeOverviewPage : ContentPage
{
    public FridgeOverviewPage(FridgeOverviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as FridgeOverviewViewModel)?.LoadStatsCommand.Execute(null);
    }
}
