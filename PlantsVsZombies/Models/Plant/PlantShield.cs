using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Models.Plant;

public class PlantShield : BasePlant
{
    public PlantShield()
    {
        var plantImages = PlantType.Shield.GetPlantImages();
        ShieldImageSource = plantImages[0];
        
        Health = ConfigService.GetConfig().Plants[nameof(PlantType.Shield)].Health;
        MaxHealth = Health;
    }
    
    public BitmapImage ShieldImageSource { get; }
    
    public override PlantType Type { get; } = PlantType.Shield;
    public override string Title { get; } = "Щит";
}