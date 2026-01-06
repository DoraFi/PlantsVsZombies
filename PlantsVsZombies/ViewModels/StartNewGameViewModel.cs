using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.ViewModels;

public partial class StartNewGameViewModel : BaseViewModel
{
    [ObservableProperty]
    private LocationType _selectedLocation = LocationType.GrassLawn;

    [ObservableProperty]
    private int _initialDifficulty = 1;

    public List<LocationType> Locations { get; } = new() { LocationType.GrassLawn, LocationType.SandBeach };

    [RelayCommand]
    private void StartGame()
    {
        var gameService = new Services.GameService();
        var session = gameService.CreateNewGame(SelectedLocation, InitialDifficulty);
        NavigationService.Instance.NavigateTo(new GameViewModel(session));
    }

    [RelayCommand]
    private void Back()
    {
        NavigationService.Instance.NavigateTo(new MainMenuViewModel());
    }
}
