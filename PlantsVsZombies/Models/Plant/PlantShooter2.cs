using System.Windows;
using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models.Plant;

public class PlantShooter2 : BasePlant
{
   
    
    public PlantShooter2()
    {
        var plantImages = PlantType.Shooter2.GetPlantImages();
        BodyImageSource = plantImages[0];
        ArmLeftImageSource = plantImages[1];
        ArmRightImageSource = plantImages[2];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Shooter2)].Health;
        MaxHealth = Health;
        _shootDelay = TimeSpan.FromSeconds(ConfigService.GetConfig().Plants[nameof(PlantType.Shooter2)].ShootDelay);
    }
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage ArmLeftImageSource { get; }
    public BitmapImage ArmRightImageSource { get; }


    private TimeSpan _shootDelay;
    private DateTime _lastShootTime = DateTime.MinValue;
    public override void MakeAction(IEnumerable<BaseZombie> zombies)
    {
        bool isZombieSuitableForShooting =
            zombies.Any(zombie => (zombie.CurrentFieldCell?.Column ?? 0) >= this.Column && zombie.Row == Row);
        if (isZombieSuitableForShooting && DateTime.Now - _lastShootTime >= _shootDelay)
        {
            _lastShootTime = DateTime.Now;
            SpawnBullet();
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                Application.Current.Dispatcher.Invoke(SpawnBullet);
            });
        }
    }

    public override PlantType Type { get; } = PlantType.Shooter2;
    public override string Title { get; } = "Пулемёт";
}