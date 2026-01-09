using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models.Plant;

public class PlantGenerator : BasePlant
{
    public PlantGenerator()
    {
        var plantImages = PlantType.Generator.GetPlantImages();
        BodyImageSource = plantImages[0];
        HeadImageSource = plantImages[1];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Generator)].Health;
        MaxHealth = Health;
        
        _spawnSunDelay = TimeSpan.FromSeconds(ConfigService.GetConfig().Game.SunFallInterval);
    }
    
    private TimeSpan _spawnSunDelay;
    private DateTime _lastShootTime = DateTime.Now;
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage HeadImageSource { get; }

    public event Action<PlantGenerator>? SunSpawnRequested;
    
    public override void MakeAction(IEnumerable<BaseZombie> zombies)
    {
        if (DateTime.Now - _lastShootTime >= _spawnSunDelay)
        {
            _lastShootTime = DateTime.Now;
            SunSpawnRequested?.Invoke(this);
        }
    }

    public override PlantType Type { get; } = PlantType.Generator;
    public override string Title { get; } = "Генератор";
}