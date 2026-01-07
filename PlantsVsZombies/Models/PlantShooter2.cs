using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models;

public class PlantShooter2 : Plant
{
    public PlantShooter2()
    {
        var plantImages = PlantType.Shooter2.GetPlantImages();
        BodyImageSource = plantImages[0];
        ArmLeftImageSource = plantImages[1];
        ArmRightImageSource = plantImages[2];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Shooter2)].Health;
    }
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage ArmLeftImageSource { get; }
    public BitmapImage ArmRightImageSource { get; }
    public override PlantType Type { get; } = PlantType.Shooter2;
    public override string Title { get; } = "Пулемёт";
}