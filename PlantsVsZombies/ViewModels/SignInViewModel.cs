using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class SignInViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    private void SignIn()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter login and password";
            return;
        }

        if (UserService.SignIn(Login, Password))
        {
            ErrorMessage = string.Empty;
            NavigationService.Instance.NavigateTo(new MainMenuViewModel());
        }
        else
        {
            ErrorMessage = "Invalid login or password";
        }
    }

    [RelayCommand]
    private void NavigateToSignUp()
    {
        NavigationService.Instance.NavigateTo(new SignUpViewModel());
    }
}
