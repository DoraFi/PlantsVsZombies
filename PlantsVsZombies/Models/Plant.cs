using System.ComponentModel;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models;

public abstract class Plant : INotifyPropertyChanged
{
    private int _health;
    private PlantState _state = PlantState.Idle;

    public FieldCell? FieldCell { get; set; }
    public int Row => FieldCell?.Row ?? 0;
    public int Column => FieldCell?.Column ?? 0;

    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            OnPropertyChanged(nameof(Health));
        }
    }

    public PlantState State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged(nameof(State));
        }
    }

    public double LastShootTime { get; set; }
    public double LastSunGenerationTime { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public abstract PlantType Type { get; }
    public abstract string Title { get; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
