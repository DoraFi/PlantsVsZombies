using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models.Plant;

public abstract partial class BasePlant : ObservableObject
{
    [ObservableProperty] private double _health;
    [ObservableProperty] private PlantState _state = PlantState.Idle;

    public event Action<BasePlant>? BulletSpawnRequested;
    public event Action<BasePlant>? KillRequested;

    public void Kill()
    {
        KillRequested?.Invoke(this);
    }
    
    protected void SpawnBullet()
    {
        BulletSpawnRequested?.Invoke(this);
    }
    
    public abstract void MakeAction(IEnumerable<BaseZombie> zombies);

    [JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public FieldCell? FieldCell
    {
        get => _fieldCell;
        set
        {
            _fieldCell = value;
            Row = value.Row;
            Column = value.Column;
        }
    }
    private FieldCell? _fieldCell;
    public int Row { get; set; }
    public int Column { get; set; }

    public double LastShootTime { get; set; }
    public double LastSunGenerationTime { get; set; }
    
    public abstract PlantType Type { get; }
    public abstract string Title { get; }
    public double MaxHealth { get; protected set; }
}
