using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Models.Zombie;

public class ZombieGirl : BaseZombie
{
    public ZombieGirl(List<FieldCell> fieldCells, int columns, int row, double cellSize, LocationType locationType)
        : base(fieldCells, columns, row, cellSize)
    {
        LocationType = locationType;
        ZombieImageSource = ZombieType.ZombieGirl.GetZombieImage(locationType);
        Health = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieGirl)].Health;
        MaxHealth = Health;
        Damage = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieGirl)].Damage;
        Speed = ConfigService.GetConfig().Zombies[nameof(ZombieType.ZombieGirl)].Speed;
    }
    
    public BitmapImage ZombieImageSource { get; }
    public LocationType LocationType { get; }
}