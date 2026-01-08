using System.Windows;
using System.Windows.Controls;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Plant;

namespace PlantsVsZombies.VisualControls;

public class FieldCell : Grid
{
    private readonly Viewbox _viewBox = new();
    
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
                }
                else
                {
                    _viewBox.Child = new ContentControl()
                    {
                        Content = Plant
                    };
                }
            });

        }
    }
    
    public required int Row { get; init; }
    public required int Column { get; init; }

    public void PlacePlant(PlantType plantType)
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
    }
}