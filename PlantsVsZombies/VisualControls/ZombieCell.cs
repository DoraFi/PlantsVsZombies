using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Zombie;

namespace PlantsVsZombies.VisualControls;

public class ZombieCell : Grid
{
    private readonly Viewbox _viewBox = new();
    private HealthBar? _healthBar;
    private double _cellSize;

    public ZombieCell(double cellSize)
    {
        _cellSize = cellSize;
        Width = cellSize * 2.25;
        Height = cellSize * 2.25;
        
        _viewBox.Width = cellSize * 2.25;
        _viewBox.Height = cellSize * 2.25;
        _viewBox.Stretch = Stretch.Uniform;
        this.Children.Add(_viewBox);
    }

    private BaseZombie? _zombie;

    public BaseZombie? Zombie
    {
        get => _zombie;
        set
        {
            _zombie = value;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (value == null)
                {
                    _viewBox.Child = null;
                    
                    // Remove health bar
                    if (_healthBar != null)
                    {
                        this.Children.Remove(_healthBar);
                        _healthBar = null;
                    }
                }
                else
                {
                    _viewBox.Child = new ContentControl()
                    {
                        Content = value
                    };
                    
                    // Add or update health bar
                    if (_healthBar == null)
                    {
                        var containerWidth = ActualWidth > 0 ? ActualWidth : Width;
                        _healthBar = new HealthBar
                        {
                            
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            RenderTransform = new TranslateTransform(-110, -20),
                            Width = _cellSize * 0.6
                        };
                        
                        _healthBar.SetValue(Panel.ZIndexProperty, 10000);
                        _healthBar.SetBinding(HealthBar.HealthProperty, new Binding(nameof(value.Health)) { Source = value, Mode = BindingMode.OneWay });
                        _healthBar.SetBinding(HealthBar.MaxHealthProperty, new Binding(nameof(value.MaxHealth)) { Source = value, Mode = BindingMode.OneWay });
                        this.Children.Add(_healthBar);
                    }
                    else
                    {
                        // Update bindings if health bar already exists
                        _healthBar.SetBinding(HealthBar.HealthProperty, new Binding(nameof(value.Health)) { Source = value, Mode = BindingMode.OneWay });
                        _healthBar.SetBinding(HealthBar.MaxHealthProperty, new Binding(nameof(value.MaxHealth)) { Source = value, Mode = BindingMode.OneWay });
                        // Update width
                        _healthBar.Width = ActualWidth * 0.6;
                    }
                }
            });
        }
    }

    public void SetZombie(BaseZombie zombie)
    {
        Zombie = zombie;
        
        // Bind position to zombie's X and Y properties
        var leftBinding = new Binding(nameof(zombie.X))
        {
            Source = zombie,
            Mode = BindingMode.OneWay
        };

        var topBinding = new Binding(nameof(zombie.Y))
        {
            Source = zombie,
            Mode = BindingMode.OneWay
        };

        this.SetBinding(Canvas.LeftProperty, leftBinding);
        this.SetBinding(Canvas.TopProperty, topBinding);
    }
}
