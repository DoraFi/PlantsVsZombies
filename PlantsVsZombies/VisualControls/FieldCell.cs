using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Plant;

namespace PlantsVsZombies.VisualControls;

public class FieldCell : Grid
{
    private readonly Viewbox _viewBox = new();
    private HealthBar? _healthBar;
    
    public FieldCell()
    {
        this.Children.Add(_viewBox);
        
    }

    private BasePlant? _plant;

    public BasePlant? Plant
    {
        get => _plant;
        set
        {
            _plant = value;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (value == null)
                {
                    _viewBox.Child = null;
                    
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
                        Content = Plant
                    };
                    
                    if (_healthBar == null)
                    {
                        var cellWidth = ActualWidth > 0 ? ActualWidth : Width;
                        _healthBar = new HealthBar
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            Width = cellWidth * 0.6,
                            
                        };
                        _healthBar.SetValue(Panel.ZIndexProperty, 10000);
                        _healthBar.SetBinding(HealthBar.HealthProperty, new Binding(nameof(value.Health)) { Source = value });
                        _healthBar.SetBinding(HealthBar.MaxHealthProperty, new Binding(nameof(value.MaxHealth)) { Source = value });
                        this.Children.Add(_healthBar);
                        
                        this.SizeChanged += (s, e) =>
                        {
                            if (_healthBar != null)
                            {
                                _healthBar.Width = ActualWidth * 0.6;
                            }
                        };
                    }
                    else
                    {
                        _healthBar.SetBinding(HealthBar.HealthProperty, new Binding(nameof(value.Health)) { Source = value });
                        _healthBar.SetBinding(HealthBar.MaxHealthProperty, new Binding(nameof(value.MaxHealth)) { Source = value });
                        _healthBar.Width = ActualWidth * 0.6;
                    }
                }
            });

        }
    }
    
    public required int Row { get; init; }
    public required int Column { get; init; }

    public BasePlant PlacePlant(PlantType plantType)
    {
        _viewBox.Width = Width / 1.9;
        _viewBox.Height = Height / 1.9;
        Plant = plantType switch
        {
            PlantType.Shield => new PlantShield() { FieldCell = this },
            PlantType.Generator => new PlantGenerator() { FieldCell = this },
            PlantType.Shooter1 => new PlantShooter1() { FieldCell = this },
            PlantType.Shooter2 => new PlantShooter2() { FieldCell = this },
            _ => throw new ArgumentOutOfRangeException(nameof(plantType), plantType, null)
        };
        return Plant;
    }
}