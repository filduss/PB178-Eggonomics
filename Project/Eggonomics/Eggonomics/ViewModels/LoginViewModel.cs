using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Eggonomics.Interfaces;
using Eggonomics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Eggonomics.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isSignUpVisible = false;

    [ObservableProperty]
    private string signUpEmail;

    [ObservableProperty]
    private string signUpPassword;

    [ObservableProperty]
    private string confirmPassword;

    [ObservableProperty]
    private string signUpError;

    [ObservableProperty]
    private bool hasSignUpError;

    public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    public void OpenSignUp()
    {
        IsSignUpVisible = true;
    }

    [RelayCommand]
    public void CloseSignUp()
    {
        IsSignUpVisible = false;
    }

    [RelayCommand]
    public async Task PerformLogin()
    {
        bool isSuccess = await _authService.LoginAsync(email, password);
        if (isSuccess)
        {
            var fridgesPage = _serviceProvider.GetRequiredService<FridgesPage>();
            Application.Current!.Windows[0].Page = fridgesPage;
        }
        else
        {
            Debug.WriteLine("Login failed");
        }
    }

    [RelayCommand]
    public async Task PerformRegister()
    {
        if (SignUpPassword?.Length < 8)
        {
            SignUpError = "Password must be at least 8 characters.";
            HasSignUpError = true;
            return;
        }

        if (SignUpPassword != ConfirmPassword)
        {
            SignUpError = "Passwords do not match.";
            HasSignUpError = true;
            return;
        }

        SignUpError = null;
        HasSignUpError = false;
        bool isSuccess = await _authService.CreateAccountAsync(SignUpEmail, SignUpPassword);
        if (isSuccess)
        {
            SignUpEmail = string.Empty;
            SignUpPassword = string.Empty;
            ConfirmPassword = string.Empty;
            IsSignUpVisible = false;
        }
        else
        {
            SignUpError = "Registration failed. Try again.";
            HasSignUpError = true;
        }
    }
}
