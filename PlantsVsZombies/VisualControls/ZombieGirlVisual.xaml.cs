using System.ComponentModel;
using System.Windows.Controls;
using PlantsVsZombies.Models.Zombie;

namespace PlantsVsZombies.VisualControls;

public partial class ZombieGirlVisual : Canvas
{
    public ZombieGirlVisual()
    {
        InitializeComponent();
        Loaded += ZombieGirlVisual_Loaded;
    }

    private void ZombieGirlVisual_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is BaseZombie zombie)
        {
            zombie.PropertyChanged += Zombie_PropertyChanged;
        }
    }

    private void Zombie_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BaseZombie.State))
        {
            if (sender is BaseZombie zombie)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\levak\RiderProjects\DoraPlantsVsZombies\.cursor\debug.log", $"{{\"location\":\"ZombieGirlVisual.cs:Zombie_PropertyChanged\",\"message\":\"State property change detected in visual\",\"newState\":\"{zombie.State}\",\"hasRotateTransform\":{Rotate != null},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
            }
        }
    }
}