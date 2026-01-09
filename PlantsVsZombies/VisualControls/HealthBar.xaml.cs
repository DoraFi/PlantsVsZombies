using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PlantsVsZombies.VisualControls;

public partial class HealthBar : UserControl
{
    public static readonly DependencyProperty HealthProperty =
        DependencyProperty.Register(nameof(Health), typeof(double), typeof(HealthBar),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaxHealthProperty =
        DependencyProperty.Register(nameof(MaxHealth), typeof(double), typeof(HealthBar),
            new PropertyMetadata(100.0));

    public double Health
    {
        get => (double)GetValue(HealthProperty);
        set => SetValue(HealthProperty, value);
    }

    public double MaxHealth
    {
        get => (double)GetValue(MaxHealthProperty);
        set => SetValue(MaxHealthProperty, value);
    }

    public HealthBar()
    {
        InitializeComponent();
    }
}

public class HealthToPercentageConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 3 || values[0] is not double health || values[1] is not double maxHealth || values[2] is not double width)
            return 0.0;

        if (maxHealth <= 0)
            return 0.0;

        var percentage = Math.Max(0, Math.Min(1, health / maxHealth));
        var fillableWidth = width - 2;
        return Math.Max(0, fillableWidth * percentage);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class HealthToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not double health || values[1] is not double maxHealth)
            return Colors.Gray;

        if (maxHealth <= 0)
            return Colors.Gray;

        var percentage = Math.Max(0, Math.Min(1, health / maxHealth));
        return GetHealthColor(percentage);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Color GetHealthColor(double percentage)
    {
        if (percentage > 0.6)
        {
            return Color.FromRgb(46, 204, 113); 
        }
        else if (percentage > 0.3)
        {
            var factor = (percentage - 0.3) / 0.3;
            return Color.FromRgb(
                (byte)(241 + (255 - 241) * (1 - factor)), 
                (byte)(196 + (152 - 196) * (1 - factor)),
                (byte)(15 + (0 - 15) * (1 - factor))  
            );
        }
        else
        {
            var factor = Math.Max(0, percentage / 0.3);
            return Color.FromRgb(
                (byte)(231 + (192 - 231) * (1 - factor)), 
                (byte)(76 + (57 - 76) * (1 - factor)),    
                (byte)(60 + (43 - 60) * (1 - factor))    
            );
        }
    }
}
