using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class MainMenuViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _hasActiveSession;

    public MainMenuViewModel()
    {
        HasActiveSession = UserService.LoadGameSession() != null;
    }

    [RelayCommand]
    private void ContinueGame()
    {
        var session = UserService.LoadGameSession();
        if (session != null)
        {
            NavigationService.Instance.NavigateTo(new GameViewModel(session));
        }
    }

    [RelayCommand]
    private void OpenInfoView()
    {
        NavigationService.Instance.NavigateTo(new GameInfoViewModel());
    }

    [RelayCommand]
    private void StartNewGame()
    {
        NavigationService.Instance.NavigateTo(new StartNewGameViewModel());
    }

    [RelayCommand]
    private void SignOut()
    {
        UserService.SignOut();
        NavigationService.Instance.NavigateTo(new SignInViewModel());
    }
}
