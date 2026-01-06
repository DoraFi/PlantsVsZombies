using System.ComponentModel;

namespace PlantsVsZombies.Models;

public class Sun : INotifyPropertyChanged
{
    private double _x;
    private double _y;

    public double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged(nameof(X));
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged(nameof(Y));
        }
    }

    public double SpawnTime { get; set; }
    public bool IsFromGenerator { get; set; }
    public double TargetY { get; set; }
    public double TargetX { get; set; }
    public double VelocityY { get; set; }
    public double StartX { get; set; }
    public double StartY { get; set; }
    public bool IsAnimating { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
