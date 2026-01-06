using System.ComponentModel;

namespace PlantsVsZombies.Models;

public class Zombie : INotifyPropertyChanged
{
    private int _health;
    private double _x;

    public ZombieType Type { get; set; }
    public int Row { get; set; }
    public double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged(nameof(X));
        }
    }

    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            OnPropertyChanged(nameof(Health));
        }
    }

    public double LastDamageTime { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
