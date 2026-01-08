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
        // Account for border margin (1 pixel on each side = 2 total)
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
            // Green when health is above 60%
            return Color.FromRgb(46, 204, 113); // Emerald green
        }
        else if (percentage > 0.3)
        {
            // Yellow/Orange when health is between 30% and 60%
            // Interpolate between yellow and orange
            var factor = (percentage - 0.3) / 0.3; // 0 to 1 when percentage goes from 0.3 to 0.6
            return Color.FromRgb(
                (byte)(241 + (255 - 241) * (1 - factor)), // Yellow to Orange red
                (byte)(196 + (152 - 196) * (1 - factor)), // Yellow to Orange green
                (byte)(15 + (0 - 15) * (1 - factor))      // Yellow to Orange blue
            );
        }
        else
        {
            // Red when health is below 30%
            // Interpolate between orange-red and deep red
            var factor = Math.Max(0, percentage / 0.3); // 0 to 1 when percentage goes from 0 to 0.3
            return Color.FromRgb(
                (byte)(231 + (192 - 231) * (1 - factor)), // Orange-red to deep red
                (byte)(76 + (57 - 76) * (1 - factor)),    // Orange-red to deep red
                (byte)(60 + (43 - 60) * (1 - factor))     // Orange-red to deep red
            );
        }
    }
}
