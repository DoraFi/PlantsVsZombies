using System.ComponentModel;

namespace PlantsVsZombies.Models;

public class Bullet : INotifyPropertyChanged
{
    private double _x;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
