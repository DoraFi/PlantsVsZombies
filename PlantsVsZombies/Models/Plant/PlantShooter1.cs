using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
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
    }
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage HeadImageSource { get; }
    
    public override PlantType Type { get; } = PlantType.Shooter1;
    public override string Title { get; } = "Монстр";
}