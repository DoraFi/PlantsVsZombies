using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Converters;

public class LocationToIndicatorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is LocationType selectedLocation && values[1] is LocationType itemLocation)
        {
            // Return highlighted color if this is the selected location
            if (selectedLocation == itemLocation)
            {
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            }
        }
        // Return default color for non-selected indicators
        return new SolidColorBrush(Color.FromRgb(200, 200, 200)); // Light gray
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
