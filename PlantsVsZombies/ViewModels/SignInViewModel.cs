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
            ErrorMessage = "Введите логин или пароль!";
            return;
        }

        if (UserService.SignIn(Login, Password))
        {
            ErrorMessage = string.Empty;
            NavigationService.Instance.NavigateTo(new MainMenuViewModel());
        }
        else
        {
            ErrorMessage = "Неправильный логин или пароль!";
        }
    }

    [RelayCommand]
    private void NavigateToSignUp()
    {
        NavigationService.Instance.NavigateTo(new SignUpViewModel());
    }
}
