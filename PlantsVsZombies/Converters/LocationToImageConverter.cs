using System.Globalization;
using System.Windows.Data;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Converters;

public class LocationToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LocationType location)
        {
            return location.GetLocationImage();
        }
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
