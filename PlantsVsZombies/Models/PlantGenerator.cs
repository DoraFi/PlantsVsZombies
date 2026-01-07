using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models;

public class PlantGenerator : Plant
{
    public PlantGenerator()
    {
        var plantImages = PlantType.Generator.GetPlantImages();
        BodyImageSource = plantImages[0];
        HeadImageSource = plantImages[1];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Generator)].Health;
    }
    
    public BitmapImage BodyImageSource { get; }
    public BitmapImage HeadImageSource { get; }
    public override PlantType Type { get; } = PlantType.Generator;
    public override string Title { get; } = "Генератор";
}