using System.Globalization;
using System.Windows.Data;
using PlantsVsZombies.Models;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Converters;

public class CanAffordConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is PlantType plantType && values[1] is GameViewModel viewModel)
        {
            return viewModel.CanAffordPlant(plantType);
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
