using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using PlantsVsZombies.Services;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models.Zombie;

public abstract partial class BaseZombie : ObservableObject
{
    private List<FieldCell> _fieldCellsOnRow;
    
    public BaseZombie(List<FieldCell> fieldCellsOnRow, int columns, int row, double cellSize)
    {
        _fieldCellsOnRow = fieldCellsOnRow;
        Row = row;
        CellSize = cellSize;
        Columns = columns;
        Y = CellSize * Row + 0.1 * CellSize;
        X = CellSize * Columns + 0.5 * CellSize;
    }

    public event Action<BaseZombie>? KillRequested;
    public void Kill()
    {
        if (_isKillRequested)
            return;
        _isKillRequested = true;
        KillRequested?.Invoke(this);
    }

    private bool _isKillRequested = false;
    
    public void MakeAction()
    {
        if (State == ZombieState.Dead)
            return;
        
        //кушать растение
        if (CurrentFieldCell != null && CurrentFieldCell.Plant != null)
        {
            CurrentFieldCell.Plant.Health -= Damage / ConfigService.GetConfig().Game.FPS;
            if (CurrentFieldCell.Plant.Health <= 0)
            {
                CurrentFieldCell.Plant.Kill();
                CurrentFieldCell.Plant = null;
            }
        }
        //двигаться дальше
        else
        {
            X -= Speed * CellSize / 120.0 / ConfigService.GetConfig().Game.FPS;
            var currentColumn = (int)Math.Ceiling((X + 0.5 * CellSize) / CellSize);
            if (currentColumn <= Columns && currentColumn >= 1)
            {
                CurrentFieldCell = _fieldCellsOnRow[currentColumn - 1];
            }
        }
    }

    [ObservableProperty]
    private double _x;
    
    [ObservableProperty]
    private double _health;

    public double MaxHealth { get; protected set; }
    
    [ObservableProperty]
    private double _speed;
    
    [ObservableProperty]
    private double _damage;
    
    [ObservableProperty]
    private FieldCell? _currentFieldCell;
    
    [ObservableProperty] 
    private ZombieState _state;
    
    partial void OnStateChanged(ZombieState value)
    {
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Users\levak\RiderProjects\DoraPlantsVsZombies\.cursor\debug.log", $"{{\"location\":\"BaseZombie.cs:OnStateChanged\",\"message\":\"State property changed\",\"newState\":\"{value}\",\"zombieType\":\"{GetType().Name}\",\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion
    }
    
    public ZombieType Type { get; set; }
    
    public int Row { get; }
    public int Columns { get; }
    public double CellSize { get; }
    public double Y { get; }
}
