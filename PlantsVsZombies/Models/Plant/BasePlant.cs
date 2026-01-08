using CommunityToolkit.Mvvm.ComponentModel;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models.Plant;

public abstract partial class BasePlant : ObservableObject
{
    [ObservableProperty] private double _health;
    [ObservableProperty] private PlantState _state = PlantState.Idle;

    public FieldCell? FieldCell { get; set; }
    public int Row => FieldCell?.Row ?? 0;
    public int Column => FieldCell?.Column ?? 0;

    public double LastShootTime { get; set; }
    public double LastSunGenerationTime { get; set; }
    
    public abstract PlantType Type { get; }
    public abstract string Title { get; }
    public double MaxHealth { get; protected set; }
}
