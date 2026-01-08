using System.ComponentModel;
using PlantsVsZombies.Models.Plant;

namespace PlantsVsZombies.Models;

public class Bullet : INotifyPropertyChanged
{
    private double _x;

    public required int Row { get; init; }
    public required double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged(nameof(X));
        }
    }
    
    public required PlantType ParentPlantType { get; init; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
