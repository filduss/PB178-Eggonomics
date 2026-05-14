using Eggonomics.ViewModels;

namespace Eggonomics.Views;

public partial class FridgePage : ContentPage
{
    public FridgePage(FridgeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as FridgeViewModel;
        if (vm == null)
        {
            return;
        }

        int expired = await vm.CheckExpiredTraysAsync();
        await vm.LoadTrays();
        await vm.LoadOperations();

        if (expired > 0)
        {
            await DisplayAlertAsync("Eggs Expired", $"{expired} egg{(expired == 1 ? "" : "s")} were removed from expired trays.", "OK");
        }
    }
}
