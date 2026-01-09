using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.VisualControls;

public class SunCell : Image
{
    private ScaleTransform? _scaleTransform;
    private RotateTransform? _rotateTransform;
    private Storyboard? _appearingStoryboard;
    private Storyboard? _idleStoryboard;
    
    public SunCell(Sun sun)
    {
        Width = 64;
        Height = 64;
        RenderTransformOrigin = new Point(0.5, 0.5);
        Cursor = System.Windows.Input.Cursors.Hand;
        Opacity = 0; // Start invisible for fade-in animation

        Source = GraphicsProvider.GetSunImage();
        
        // Setup transforms for animations
        var transformGroup = new TransformGroup();
        _scaleTransform = new ScaleTransform(0.3, 0.3); // Start smaller for pop-in effect
        _rotateTransform = new RotateTransform();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_rotateTransform);
        RenderTransform = transformGroup;
        
        // Start animations after element is loaded (when name scope is available)
        Loaded += SunCell_Loaded;
    }
    
    private void SunCell_Loaded(object sender, RoutedEventArgs e)
    {
        if (_scaleTransform != null && _rotateTransform != null && _appearingStoryboard == null)
        {
            // Register transforms with names so we can target them in animations
            // Must be done after element is in visual tree (name scope available)
            // Use unique names based on instance hash code to avoid conflicts
            var scaleName = $"ScaleTransform_{GetHashCode()}";
            var rotateName = $"RotateTransform_{GetHashCode()}";
            
            try
            {
                RegisterName(scaleName, _scaleTransform);
                RegisterName(rotateName, _rotateTransform);
            }
            catch (ArgumentException)
            {
                // Names already registered, try to unregister first
                try { UnregisterName(scaleName); } catch { }
                try { UnregisterName(rotateName); } catch { }
                RegisterName(scaleName, _scaleTransform);
                RegisterName(rotateName, _rotateTransform);
            }
            
            // Start appearing animation
            StartAppearingAnimation(_scaleTransform, _rotateTransform, scaleName, rotateName);
            
            // Start subtle idle animation (gentle rotation and pulse)
            StartIdleAnimation(_scaleTransform, _rotateTransform, scaleName, rotateName);
        }
    }
    
    private void StartAppearingAnimation(ScaleTransform scaleTransform, RotateTransform rotateTransform, string scaleName, string rotateName)
    {
        // Store storyboard to prevent garbage collection
        _appearingStoryboard = new Storyboard();
        
        // Scale up animation (pop-in effect) - animate from current value (0.3) to 1.2
        var scaleXAnimation = new DoubleAnimation
        {
            To = 1.2,
            Duration = new Duration(TimeSpan.FromSeconds(0.15)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetName(scaleXAnimation, scaleName);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        _appearingStoryboard.Children.Add(scaleXAnimation);
        
        var scaleYAnimation = new DoubleAnimation
        {
            To = 1.2,
            Duration = new Duration(TimeSpan.FromSeconds(0.15)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetName(scaleYAnimation, scaleName);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        _appearingStoryboard.Children.Add(scaleYAnimation);
        
        // Bounce back to normal size
        var bounceScaleXAnimation = new DoubleAnimation
        {
            From = 1.2,
            To = 1.0,
            BeginTime = TimeSpan.FromSeconds(0.15),
            Duration = new Duration(TimeSpan.FromSeconds(0.2)),
            EasingFunction = new ElasticEase 
            { 
                EasingMode = EasingMode.EaseOut,
                Oscillations = 1,
                Springiness = 3
            }
        };
        Storyboard.SetTargetName(bounceScaleXAnimation, scaleName);
        Storyboard.SetTargetProperty(bounceScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        _appearingStoryboard.Children.Add(bounceScaleXAnimation);
        
        var bounceScaleYAnimation = new DoubleAnimation
        {
            From = 1.2,
            To = 1.0,
            BeginTime = TimeSpan.FromSeconds(0.15),
            Duration = new Duration(TimeSpan.FromSeconds(0.2)),
            EasingFunction = new ElasticEase 
            { 
                EasingMode = EasingMode.EaseOut,
                Oscillations = 1,
                Springiness = 3
            }
        };
        Storyboard.SetTargetName(bounceScaleYAnimation, scaleName);
        Storyboard.SetTargetProperty(bounceScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        _appearingStoryboard.Children.Add(bounceScaleYAnimation);
        
        // Fade in animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(0.25))
        };
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));
        _appearingStoryboard.Children.Add(opacityAnimation);
        
        // Gentle rotation during appearance
        var rotationAnimation = new DoubleAnimation
        {
            From = -15,
            To = 15,
            Duration = new Duration(TimeSpan.FromSeconds(0.35)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTargetName(rotationAnimation, rotateName);
        Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
        _appearingStoryboard.Children.Add(rotationAnimation);
        
        _appearingStoryboard.Begin(this);
    }
    
    private void StartIdleAnimation(ScaleTransform scaleTransform, RotateTransform rotateTransform, string scaleName, string rotateName)
    {
        // Wait for appearing animation to complete, then start idle animation
        var delay = TimeSpan.FromSeconds(0.4);
        
        // Store storyboard to prevent garbage collection
        _idleStoryboard = new Storyboard();
        _idleStoryboard.BeginTime = delay;
        
        // Gentle continuous rotation
        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(8)),
            RepeatBehavior = RepeatBehavior.Forever
        };
        Storyboard.SetTargetName(rotateAnimation, rotateName);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(RotateTransform.AngleProperty));
        _idleStoryboard.Children.Add(rotateAnimation);
        
        // Subtle pulsing (breathing effect)
        var pulseScaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.08,
            Duration = new Duration(TimeSpan.FromSeconds(1.5)),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTargetName(pulseScaleAnimation, scaleName);
        Storyboard.SetTargetProperty(pulseScaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        _idleStoryboard.Children.Add(pulseScaleAnimation);
        
        var pulseScaleYAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.08,
            Duration = new Duration(TimeSpan.FromSeconds(1.5)),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTargetName(pulseScaleYAnimation, scaleName);
        Storyboard.SetTargetProperty(pulseScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        _idleStoryboard.Children.Add(pulseScaleYAnimation);
        
        _idleStoryboard.Begin(this);
    }
}