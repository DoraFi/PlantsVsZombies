using System.Windows.Controls;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.VisualControls;

public class FieldCell : Grid
{
    private ContentControl _contentControl;
    
    public FieldCell()
    {
        _contentControl = new ContentControl();
        this.Children.Add(_contentControl);
    }
    
    public required int Row { get; init; }
    public required int Column { get; init; }

    private Plant? _plant;
    public Plant? Plant
    {
        get => _plant;
        private set
        {
            _plant = value;
            _contentControl.Content = value;
        }
    }

    public void PlacePlant(PlantType plantType)
    {
        Plant = plantType switch
        {
            PlantType.Shield => new PlantShield() { FieldCell = this },
            PlantType.Generator => new PlantGenerator() { FieldCell = this },
            PlantType.Shooter1 => new PlantShooter1() { FieldCell = this },
            PlantType.Shooter2 => new PlantShooter2() { FieldCell = this },
        };
    }
}