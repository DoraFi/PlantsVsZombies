using System.ComponentModel;

namespace PlantsVsZombies.Models;

public class Plant : INotifyPropertyChanged
{
    private int _health;
    private PlantState _state = PlantState.Idle;

    public PlantType Type { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
