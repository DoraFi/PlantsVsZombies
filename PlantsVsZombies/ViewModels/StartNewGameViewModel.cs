using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class StartNewGameViewModel : BaseViewModel
{
    [ObservableProperty]
    private LocationType _selectedLocation = LocationType.GrassLawn;

    [ObservableProperty] private int _startDifficulty = ConfigService.GetConfig().Game.InitialDifficulty;

    public List<LocationType> Locations { get; } = new() { LocationType.GrassLawn, LocationType.SandBeach };

    [RelayCommand]
    private void StartGame()
    {
        if (StartDifficulty < 1)
        {
            MessageBox.Show("Начальная сложность не может быть меньше 1!");
            return;
        }
        
        var gameService = new Services.GameService();
        
        var session = gameService.CreateNewGame(SelectedLocation, StartDifficulty);
        NavigationService.Instance.NavigateTo(new GameViewModel(session));
    }
    
    

    [RelayCommand]
    private void Back()
    {
        NavigationService.Instance.NavigateTo(new MainMenuViewModel());
    }
}
