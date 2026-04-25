using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Eggonomics.Interfaces;

namespace Eggonomics.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task PerformLogin()
    {
        bool isSuccess = await _authService.LoginAsync(email, password);

        Debug.WriteLine($"worked: {isSuccess}");
    }
}
