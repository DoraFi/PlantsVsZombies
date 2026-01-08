using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models.Zombie;

public class ZombieBoy : BaseZombie
{
    public ZombieBoy(List<FieldCell> fieldCells, int columns, int row, double cellSize, LocationType locationType)
        : base(fieldCells, columns, row, cellSize)
    {
        LocationType = locationType;
        ZombieImageSource = ZombieType.ZombieBoy.GetZombieImage(locationType);
        Health = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieBoy)].Health;
        MaxHealth = Health;
        Damage = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieBoy)].Damage;
        Speed = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieBoy)].Speed;
    }
    
    public BitmapImage ZombieImageSource { get; }
    public LocationType LocationType { get; }
}