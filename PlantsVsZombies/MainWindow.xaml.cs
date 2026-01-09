using System.Windows;
using System.Windows.Controls;
using PlantsVsZombies.Services;
using PlantsVsZombies.ViewModels;
using PlantsVsZombies.Views;

namespace PlantsVsZombies;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupNavigation();
        NavigateToInitialView();
    }

    private void SetupNavigation()
    {
        NavigationService.Instance.NavigationRequested += OnNavigationRequested;
    }

    private void OnNavigationRequested(BaseViewModel viewModel)
    {
        UserControl? view = viewModel switch
        {
            SignInViewModel => new SignInView(),
            SignUpViewModel => new SignUpView(),
            MainMenuViewModel => new MainMenuView { DataContext = (MainMenuViewModel)viewModel },
            StartNewGameViewModel => new StartNewGameView { DataContext = (StartNewGameViewModel)viewModel },
            GameViewModel => new GameView((GameViewModel)viewModel),
            GameOverViewModel => new GameOverView { DataContext = (GameOverViewModel)viewModel },
            _ => null
        };

        if (view != null)
        {
            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(view);
        }
    }

    private void NavigateToInitialView()
    {
        // Check if user is already signed in
        var currentUser = UserService.CurrentUser;
        if (currentUser != null)
        {
            NavigationService.Instance.NavigateTo(new MainMenuViewModel());
        }
        else
        {
            NavigationService.Instance.NavigateTo(new SignInViewModel());
        }
    }
}
