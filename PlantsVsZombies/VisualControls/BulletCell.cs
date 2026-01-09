using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Plant;

namespace PlantsVsZombies.VisualControls;

public class BulletCell : Image
{
    private RotateTransform? _rotateTransform;
    private Storyboard? _rotationStoryboard;
    
    public BulletCell(Bullet bullet)
    {
        Width = 48;
        Height = 48;
        RenderTransformOrigin = new Point(0.5, 0.5);
        
        // Setup rotation transform
        _rotateTransform = new RotateTransform();
        RenderTransform = _rotateTransform;
        
        Source = bullet.ParentPlantType switch
        {
            PlantType.Shooter1 => new BitmapImage(
                new Uri("pack://application:,,,/Assets/Icons/shooter1_bullet.png")),
            PlantType.Shooter2 => new BitmapImage(
                new Uri("pack://application:,,,/Assets/Icons/shooter2_bullet.png")),
            _ => throw new NotImplementedException(),
        };
        
        SetBullet(bullet);
        
        // Start animation after element is loaded (when name scope is available)
        Loaded += BulletCell_Loaded;
    }
    
    private void BulletCell_Loaded(object sender, RoutedEventArgs e)
    {
        StartSpinningAnimation();
    }
    
    private void StartSpinningAnimation()
    {
        if (_rotateTransform == null || _rotationStoryboard != null) return; // Prevent multiple calls
        
        // Create and store storyboard to prevent garbage collection
        _rotationStoryboard = new Storyboard();
        
        // Use property path directly instead of named targets (avoids name registration issues)
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(0.5)),
            RepeatBehavior = RepeatBehavior.Forever
        };
        
        // Target the transform directly using property path from the Image element
        Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
        
        _rotationStoryboard.Children.Add(animation);
        _rotationStoryboard.Begin(this, true);
    }
    
    public void SetBullet(Bullet bullet)
    {
        // Bind position to bullet's X property
        var leftBinding = new Binding(nameof(bullet.X))
        {
            Source = bullet,
            Mode = BindingMode.OneWay
        };
        this.SetBinding(Canvas.LeftProperty, leftBinding);
    }
}