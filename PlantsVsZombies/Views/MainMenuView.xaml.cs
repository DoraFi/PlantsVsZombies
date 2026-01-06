using System.Windows.Controls;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class MainMenuView : UserControl
{
    public MainMenuViewModel? ViewModel { get; private set; }

    public MainMenuView()
    {
        InitializeComponent();
        Loaded += MainMenuView_Loaded;
    }

    private void MainMenuView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Get the ViewModel from DataContext (set by MainWindow) or create new one
        ViewModel = DataContext as MainMenuViewModel ?? new MainMenuViewModel();
        if (DataContext == null)
        {
            DataContext = ViewModel;
        }
    }
}
