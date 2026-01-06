using System.Windows.Controls;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class StartNewGameView : UserControl
{
    public StartNewGameViewModel? ViewModel { get; private set; }

    public StartNewGameView()
    {
        InitializeComponent();
        Loaded += StartNewGameView_Loaded;
    }

    private void StartNewGameView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Get the ViewModel from DataContext (set by MainWindow) or create new one
        ViewModel = DataContext as StartNewGameViewModel ?? new StartNewGameViewModel();
        if (DataContext == null)
        {
            DataContext = ViewModel;
        }
    }
}
