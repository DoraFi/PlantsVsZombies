using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class GameView : UserControl
{
    private readonly GameViewModel _viewModel;
    private readonly GameConfig _config;
    private PlantType? _draggedPlantType;
    private bool _isDragging;
    private Point _dragStartPoint;

    public GameView(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _config = ConfigService.GetConfig();
        
        InitializeGameField();
        SetupEventHandlers();
        StartRendering();
    }

    private void InitializeGameField()
    {
        var cellSize = _config.Field.CellSize;
        var rows = _config.Field.Rows;
        var cols = _config.Field.Columns;
        
        GameField.Width = cols * cellSize;
        GameField.Height = rows * cellSize;
        
        // Draw grid lines
        for (int i = 0; i <= rows; i++)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = i * cellSize,
                X2 = cols * cellSize,
                Y2 = i * cellSize,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            GameField.Children.Add(line);
        }
        
        for (int i = 0; i <= cols; i++)
        {
            var line = new Line
            {
                X1 = i * cellSize,
                Y1 = 0,
                X2 = i * cellSize,
                Y2 = rows * cellSize,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            GameField.Children.Add(line);
        }
    }

    private void SetupEventHandlers()
    {
        _viewModel.Plants.CollectionChanged += (s, e) => RenderGame();
        _viewModel.Zombies.CollectionChanged += (s, e) => RenderGame();
        _viewModel.Bullets.CollectionChanged += (s, e) => RenderGame();
        _viewModel.Suns.CollectionChanged += (s, e) => RenderGame();
        
        // Listen to property changes for position updates
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Session) || 
                e.PropertyName == nameof(_viewModel.Session.SunBalance))
            {
                RenderGame();
                UpdatePlantShopVisuals();
            }
        };
        
        // Update plant shop when sun balance changes
        if (_viewModel.Session != null)
        {
            _viewModel.Session.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.Session.SunBalance))
                {
                    UpdatePlantShopVisuals();
                }
            };
        }
    }
    
    private void UpdatePlantShopVisuals()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Find all plant shop items and update their visual state
            var itemsControl = LogicalTreeHelper.FindLogicalNode(this, "PlantShopItemsControl") as ItemsControl;
            if (itemsControl == null)
            {
                // Try to find it by traversing
                itemsControl = FindVisualChild<ItemsControl>(this);
            }
            
            if (itemsControl != null)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (item is PlantType plantType)
                    {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                        if (container != null)
                        {
                            var border = FindVisualChild<Border>(container);
                            if (border != null)
                            {
                                var canAfford = _viewModel.CanAffordPlant(plantType);
                                border.Opacity = canAfford ? 1.0 : 0.5;
                                border.Background = canAfford ? new SolidColorBrush(Color.FromRgb(45, 80, 22)) : new SolidColorBrush(Color.FromRgb(102, 102, 102));
                            }
                        }
                    }
                }
            }
        });
    }
    
    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }

    private void StartRendering()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        timer.Tick += (s, e) => RenderGame();
        timer.Start();
    }

    private void RenderGame()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Clear previous game objects (keep grid lines)
            var toRemove = GameField.Children.OfType<FrameworkElement>()
                .Where(c => c is not Line && c.Tag?.ToString() != "GridLine")
                .ToList();
            foreach (var element in toRemove)
            {
                GameField.Children.Remove(element);
            }

            var cellSize = _config.Field.CellSize;

            // Render plants
            foreach (var plant in _viewModel.Plants)
            {
                var rect = new Rectangle
                {
                    Width = cellSize - 4,
                    Height = cellSize - 4,
                    Fill = GetPlantColor(plant.Type),
                    Stroke = plant.State == PlantState.Active ? Brushes.Orange : Brushes.Black,
                    StrokeThickness = plant.State == PlantState.Active ? 3 : 2,
                    Tag = "Plant"
                };
                Canvas.SetLeft(rect, plant.Column * cellSize + 2);
                Canvas.SetTop(rect, plant.Row * cellSize + 2);
                GameField.Children.Add(rect);
                
                var text = new TextBlock
                {
                    Text = plant.Type.ToString().Substring(0, Math.Min(3, plant.Type.ToString().Length)),
                    FontSize = 10,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = "Plant"
                };
                Canvas.SetLeft(text, plant.Column * cellSize + cellSize / 2 - 15);
                Canvas.SetTop(text, plant.Row * cellSize + cellSize / 2 - 8);
                GameField.Children.Add(text);
            }

            // Render zombies
            foreach (var zombie in _viewModel.Zombies)
            {
                var rect = new Rectangle
                {
                    Width = cellSize - 4,
                    Height = cellSize - 4,
                    Fill = GetZombieColor(zombie.Type),
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Tag = "Zombie"
                };
                Canvas.SetLeft(rect, zombie.X);
                Canvas.SetTop(rect, zombie.Row * cellSize + 2);
                GameField.Children.Add(rect);
            }

            // Render bullets
            foreach (var bullet in _viewModel.Bullets)
            {
                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Yellow,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 1,
                    Tag = "Bullet"
                };
                Canvas.SetLeft(ellipse, bullet.X - 5);
                Canvas.SetTop(ellipse, bullet.Row * cellSize + cellSize / 2 - 5);
                GameField.Children.Add(ellipse);
            }

            // Render suns with animations
            foreach (var sun in _viewModel.Suns)
            {
                var ellipse = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new RadialGradientBrush
                    {
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Colors.Yellow, 0.0),
                            new GradientStop(Colors.Orange, 1.0)
                        }
                    },
                    Stroke = Brushes.Orange,
                    StrokeThickness = 2,
                    Cursor = Cursors.Hand,
                    Tag = "Sun"
                };
                Canvas.SetLeft(ellipse, sun.X);
                Canvas.SetTop(ellipse, sun.Y);
                ellipse.MouseLeftButtonDown += (s, e) =>
                {
                    var pos = e.GetPosition(GameField);
                    _viewModel.PickupSun(pos.X, pos.Y);
                };
                GameField.Children.Add(ellipse);
            }
        });
    }

    private Brush GetPlantColor(PlantType type)
    {
        return type switch
        {
            PlantType.Shooter1 => Brushes.Green,
            PlantType.Shooter2 => Brushes.DarkGreen,
            PlantType.Shield => Brushes.Brown,
            PlantType.Generator => Brushes.Yellow,
            _ => Brushes.Gray
        };
    }

    private Brush GetZombieColor(ZombieType type)
    {
        return type switch
        {
            ZombieType.GirlZombie => Brushes.Pink,
            ZombieType.BoyZombie => Brushes.Blue,
            _ => Brushes.Gray
        };
    }

    private void PlantShopItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is PlantType plantType)
        {
            if (!_viewModel.CanAffordPlant(plantType))
                return;

            _draggedPlantType = plantType;
            _isDragging = false;
            _dragStartPoint = e.GetPosition(this);
            element.CaptureMouse();
        }
    }

    private void PlantShopItem_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.IsMouseCaptured && _draggedPlantType.HasValue)
        {
            var currentPoint = e.GetPosition(this);
            var distance = Math.Sqrt(Math.Pow(currentPoint.X - _dragStartPoint.X, 2) + 
                                   Math.Pow(currentPoint.Y - _dragStartPoint.Y, 2));
            
            if (distance > 5) // Start dragging after 5 pixels
            {
                _isDragging = true;
                DragDrop.DoDragDrop(element, _draggedPlantType.Value, DragDropEffects.Move);
                element.ReleaseMouseCapture();
                _draggedPlantType = null;
                _isDragging = false;
            }
        }
    }

    private void PlantShopItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.IsMouseCaptured)
        {
            element.ReleaseMouseCapture();
            _draggedPlantType = null;
            _isDragging = false;
        }
    }

    private void GameField_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlantType)))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
    }

    private void GameField_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlantType)))
        {
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            var pos = e.GetPosition(GameField);
            var cellSize = _config.Field.CellSize;
            var row = (int)(pos.Y / cellSize);
            var col = (int)(pos.X / cellSize);

            if (row >= 0 && row < _config.Field.Rows && col >= 0 && col < _config.Field.Columns)
            {
                _viewModel.PlacePlant(row, col);
            }
        }
    }
}
