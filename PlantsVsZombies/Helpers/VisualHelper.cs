using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SharpVectors.Converters;

namespace PlantsVsZombies.Helpers;

public static class VisualHelper
{
    public static void StartSpinningAnimation(this FrameworkElement visual)
    {
        visual.RenderTransformOrigin = new Point(0.5, 0.5);
        
        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(1), // One full rotation per second
            RepeatBehavior = RepeatBehavior.Forever
        };

        var rotateTransform = visual.RenderTransform as RotateTransform;
        if (rotateTransform == null)
        {
            rotateTransform = new RotateTransform();
            visual.RenderTransform = rotateTransform;
        }

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
    }
}