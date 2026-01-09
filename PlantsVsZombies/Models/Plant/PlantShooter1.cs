using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models.Plant;

public class PlantShooter1 : BasePlant
{
    public PlantShooter1()
    {
        var plantImages = PlantType.Shooter1.GetPlantImages();
        BodyImageSource = plantImages[0];
        HeadImageSource = plantImages[1];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Shooter1)].Health;
        MaxHealth = Health;
        
        _shootDelay = TimeSpan.FromSeconds(ConfigService.GetConfig().Plants[nameof(PlantType.Shooter2)].ShootDelay);
    }
    
    private TimeSpan _shootDelay;
    private DateTime _lastShootTime = DateTime.MinValue;
    public override void MakeAction(IEnumerable<BaseZombie> zombies)
    {
        bool isZombieSuitableForShooting =
            zombies.Any(zombie => (zombie.CurrentFieldCell?.Column ?? 0) >= this.Column && zombie.Row == Row);
        if (isZombieSuitableForShooting && DateTime.Now - _lastShootTime >= _shootDelay)
        {
            SpawnBullet();
            _lastShootTime = DateTime.Now;
        }
    }
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage HeadImageSource { get; }
    
    public override PlantType Type { get; } = PlantType.Shooter1;
    public override string Title { get; } = "Монстр";
}