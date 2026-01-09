using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using PlantsVsZombies.Models.Plant;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models;

public partial class Bullet : ObservableObject
{
    [ObservableProperty]
    private double _x;

    private double _speed;
    private double _damage;

    public required int Row { get; init; }

    public Bullet()
    {
        _speed = ConfigService.GetConfig().Plants[ParentPlantType.ToString()].BulletSpeed;
        _damage = ConfigService.GetConfig().Plants[ParentPlantType.ToString()].BulletDamage;
    }

    public event Action<Bullet>? KillRequested;
    private volatile bool _isKillRequested;
    
    public void MakeAction(IEnumerable<BaseZombie> zombies)
    {
        if (_isKillRequested)
            return;
        
        X += _speed / ConfigService.GetConfig().Game.FPS;
        var closestZombie = zombies
            .Where(x => x.Row == Row)
            .OrderBy(zombie => zombie.X - X).FirstOrDefault(zombie => zombie.X - X > 0);
        if (closestZombie != null)
        {
            var diff = closestZombie.X - X;
            if (diff <= 2 * _speed / ConfigService.GetConfig().Game.FPS)
            {
                closestZombie.Health -= _damage;
                if (!_isKillRequested)
                {
                    _isKillRequested = true;
                    KillRequested?.Invoke(this);
                    if (closestZombie.Health <= 0)
                    {
                        closestZombie.State = ZombieState.Dead;
                        _ = Task.Run(async() =>
                        {
                            await Task.Delay(1000);
                            closestZombie.Kill();
                        });
                    }
                }
            }
        }
    }
    
    public required PlantType ParentPlantType { get; init; }
}
