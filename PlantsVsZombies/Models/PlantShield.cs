using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models;

public class PlantShield : Plant
{
    public PlantShield()
    {
        var plantImages = PlantType.Shield.GetPlantImages();
        ShieldImageSource = plantImages[0];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Shield)].Health;
    }
    
    public BitmapImage ShieldImageSource { get; }
    
    public override PlantType Type { get; } = PlantType.Shield;
    public override string Title { get; } = "Щит";
}