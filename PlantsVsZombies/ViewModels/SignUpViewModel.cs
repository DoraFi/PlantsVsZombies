using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class SignUpViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    private void SignUp()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Пожалуйста, введите логин или пароль";
            return;
        }

        if (UserService.SignUp(Login, Password))
        {
            ErrorMessage = string.Empty;
            // After successful sign up, automatically sign in and navigate to main menu
            if (UserService.SignIn(Login, Password))
            {
                NavigationService.Instance.NavigateTo(new MainMenuViewModel());
            }
        }
        else
        {
            ErrorMessage = "Пользователь уже существует";
        }
    }

    [RelayCommand]
    private void BackToSignIn()
    {
        NavigationService.Instance.NavigateTo(new SignInViewModel());
    }
}
