using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Helpers;

public static class GraphicsProvider
{
    public static BitmapImage[] GetPlantImages(this PlantType plantType) => plantType switch
    {
        PlantType.Generator => 
        [
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/generator_body.png")),
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/generator_head.png")),
        ],
        PlantType.Shield => 
        [
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shield_body.png")),
        ],
        PlantType.Shooter1 => 
        [
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shooter1_body.png")),
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shooter1_head.png")),
        ],
        PlantType.Shooter2 => 
        [
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shooter2_body.png")),
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shooter2_hand_left.png")),
            new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/shooter2_hand_right.png")),
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(plantType), plantType, null)
    };
    
    public static BitmapImage GetLocationRoofImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/BasicRoof.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/SandRoof.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static BitmapImage GetLocationImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/Basic.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/Sand.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static BitmapImage GetBushLeftImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/BasicBushLeft.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/SandBushLeft.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static BitmapImage GetBushTopImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/BasicBushTopBottom.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/SandBushTopBottom.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static BitmapImage GetBushRightImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/BasicBushRight.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/SandBushRight.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static BitmapImage GetBushBottomImage(this LocationType locationType) => locationType switch
    {
        LocationType.GrassLawn => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/BasicBushTopBottom.png")),
        LocationType.SandBeach => new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/SandBushTopBottom.png")),
        _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
    };
    
    public static Brush GetLocationCellColor(this LocationType locationType, int row, int column)
    {
        int type = row % 2 + column % 2;
        switch (locationType)
        {
            case LocationType.SandBeach:
                return type switch
                {
                    0 => new SolidColorBrush(Color.FromRgb(244, 217, 164)),
                    1 => new SolidColorBrush(Color.FromRgb(231, 203, 151)),
                    2 => new SolidColorBrush(Color.FromRgb(221, 192, 141)),
                                                                                                                _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
                };
            case LocationType.GrassLawn:
                return type switch
                {
                    0 => new SolidColorBrush(Color.FromRgb(114, 190, 107)),
                    1 => new SolidColorBrush(Color.FromRgb(108, 183, 101)),
                    2 => new SolidColorBrush(Color.FromRgb(103, 178, 96)),
                    _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
                };
            default: throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null);
        }
    }
}