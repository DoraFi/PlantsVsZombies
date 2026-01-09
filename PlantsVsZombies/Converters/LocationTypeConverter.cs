using System.Globalization;
using System.Windows.Data;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Converters;

public class LocationTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LocationType location)
        {
            return location switch
            {
                LocationType.SandBeach => "Песочный пляж",
                LocationType.GrassLawn => "Стандартный",
                _ => location.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
