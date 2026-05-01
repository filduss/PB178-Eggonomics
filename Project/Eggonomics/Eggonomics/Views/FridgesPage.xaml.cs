using Eggonomics.ViewModels;

namespace Eggonomics.Views;

public partial class FridgesPage : ContentPage
{
    public FridgesPage(FridgesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as FridgesViewModel)?.LoadFridgesCommand.Execute(null);
    }
}
