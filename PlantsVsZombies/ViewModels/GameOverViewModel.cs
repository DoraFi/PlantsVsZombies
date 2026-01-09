using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class GameOverViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _finalScore;

    public GameOverViewModel()
    {

    }

    [RelayCommand]
    private void BackToMainMenu()
    {
        NavigationService.Instance.NavigateTo(new MainMenuViewModel());
    }

    [RelayCommand]
    private void PlayAgain()
    {
        NavigationService.Instance.NavigateTo(new StartNewGameViewModel());
    }
}
