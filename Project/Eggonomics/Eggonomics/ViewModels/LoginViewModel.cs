using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eggonomics.Interfaces;
using Eggonomics.Views;
using System.Diagnostics;

namespace Eggonomics.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] public partial string? Email { get; set; }
    [ObservableProperty] public partial string? Password { get; set; }
    [ObservableProperty] public partial bool IsSignUpVisible { get; set; }
    [ObservableProperty] public partial string? SignUpDisplayName { get; set; }
    [ObservableProperty] public partial string? SignUpEmail { get; set; }
    [ObservableProperty] public partial string? SignUpPassword { get; set; }
    [ObservableProperty] public partial string? ConfirmPassword { get; set; }
    [ObservableProperty] public partial string? SignUpError { get; set; }
    [ObservableProperty] public partial bool HasSignUpError { get; set; }

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
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            return;
        }

        bool isSuccess = await _authService.LoginAsync(Email, Password);
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
        if (string.IsNullOrWhiteSpace(SignUpDisplayName))
        {
            SignUpError = "Display name is required.";
            HasSignUpError = true;
            return;
        }

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
        if (SignUpEmail == null || SignUpPassword == null)
        {
            SignUpError = "Required field missing.";
            return;
        }

        string? error = await _authService.CreateAccountAsync(SignUpEmail, SignUpPassword, SignUpDisplayName);
        if (error == null)
        {
            SignUpDisplayName = string.Empty;
            SignUpEmail = string.Empty;
            SignUpPassword = string.Empty;
            ConfirmPassword = string.Empty;
            IsSignUpVisible = false;
        }
        else
        {
            SignUpError = error;
            HasSignUpError = true;
        }
    }
}
