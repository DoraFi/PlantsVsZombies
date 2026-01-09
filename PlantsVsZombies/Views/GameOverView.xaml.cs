using System.Windows.Controls;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class GameOverView : UserControl
{
    public GameOverViewModel? ViewModel { get; private set; }

    public GameOverView()
    {
        InitializeComponent();
        Loaded += GameOverView_Loaded;
    }

    private void GameOverView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ViewModel = DataContext as GameOverViewModel ?? new GameOverViewModel();
        if (DataContext == null)
        {
            DataContext = ViewModel;
        }
    }
}
