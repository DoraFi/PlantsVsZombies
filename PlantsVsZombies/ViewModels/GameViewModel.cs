using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.ViewModels;

public partial class GameViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly DispatcherTimer _gameTimer;
    private readonly DateTime _startTime;

    [ObservableProperty]
    private GameSession _session = null!;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isMenuVisible;

    [ObservableProperty]
    private PlantType? _selectedPlantType;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private bool _showNotification;

    public ObservableCollection<Plant> Plants => Session.Plants;
    public ObservableCollection<Zombie> Zombies => Session.Zombies;
    public ObservableCollection<Bullet> Bullets => Session.Bullets;
    public ObservableCollection<Sun> Suns => Session.Suns;

    public List<PlantType> AvailablePlants { get; } = new()
    {
        PlantType.Shooter1,
        PlantType.Shooter2,
        PlantType.Shield,
        PlantType.Generator
    };

    public bool CanAffordPlant(PlantType plantType)
    {
        return _gameService.CanAffordPlant(Session, plantType);
    }

    public GameViewModel(GameSession session)
    {
        _gameService = new GameService();
        _session = session;
        _startTime = DateTime.Now;

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();
    }

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuVisible = !IsMenuVisible;
    }

    [RelayCommand]
    private void ContinueGame()
    {
        IsMenuVisible = false;
    }

    [RelayCommand]
    private void QuitGame()
    {
        _gameTimer.Stop();
        UserService.SaveGameSession(Session);
        NavigationService.Instance.NavigateTo(new MainMenuViewModel());
    }

    [RelayCommand]
    private void SelectPlant(PlantType? plantType)
    {
        if (plantType.HasValue)
        {
            SelectedPlantType = plantType.Value;
        }
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (IsPaused || IsMenuVisible)
            return;

        var currentTime = (DateTime.Now - _startTime).TotalSeconds;
        var deltaTime = 0.016; // ~16ms

        _gameService.UpdateGame(Session, currentTime, deltaTime);

        // Check for game over
        if (Session.Score < 0)
        {
            _gameTimer.Stop();
            EndGame();
            return;
        }

        // Check for new best score
        var currentUser = UserService.CurrentUser;
        if (currentUser != null && currentUser.TopScores.Count > 0)
        {
            var bestScore = currentUser.TopScores.Max();
            if (Session.Score > bestScore)
            {
                ShowBestScoreNotification();
            }
        }

        OnPropertyChanged(nameof(Session));
        OnPropertyChanged(nameof(Plants));
        OnPropertyChanged(nameof(Zombies));
        OnPropertyChanged(nameof(Bullets));
        OnPropertyChanged(nameof(Suns));
    }

    public void PlacePlant(PlantType plantType, int row, int column)
    {
        if (_gameService.CanPlacePlant(Session, row, column, plantType))
        {
            _gameService.PlacePlant(Session, row, column, plantType);
            OnPropertyChanged(nameof(Plants));
        }
    }

    public void PickupSun(double x, double y)
    {
        var currentTime = (DateTime.Now - _startTime).TotalSeconds;
        if (_gameService.PickupSun(Session, x, y, currentTime))
        {
            OnPropertyChanged(nameof(Suns));
            OnPropertyChanged(nameof(Session));
        }
    }

    private void EndGame()
    {
        _gameTimer.Stop();
        if (Session.Score > 0)
        {
            UserService.UpdateUserScore(Session.Score);
        }
        UserService.ClearGameSession();
        NavigationService.Instance.NavigateTo(new MainMenuViewModel());
    }

    private void ShowBestScoreNotification()
    {
        NotificationMessage = "New Best Score!";
        ShowNotification = true;
        Task.Delay(3000).ContinueWith(_ =>
        {
            Application.Current.Dispatcher.Invoke(() => ShowNotification = false);
        });
    }
}
