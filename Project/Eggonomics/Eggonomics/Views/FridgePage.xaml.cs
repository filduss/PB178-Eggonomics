using Eggonomics.ViewModels;

namespace Eggonomics.Views;

public partial class FridgePage : ContentPage
{
    public FridgePage(FridgeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
