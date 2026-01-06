using System.Globalization;
using System.Windows.Data;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Converters;

public class PlantCostConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PlantType plantType)
        {
            try
            {
                var config = ConfigService.GetConfig();
                var plantConfig = config.Plants[plantType.ToString()];
                return $"${plantConfig.Cost}";
            }
            catch
            {
                return "";
            }
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
