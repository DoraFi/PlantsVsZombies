using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;
using PlantsVsZombies.ViewModels;
using PlantsVsZombies.VisualControls;

namespace PlantsVsZombies.Views;

public partial class GameView : UserControl
{
    private readonly GameViewModel _viewModel;
    private readonly GameConfig _config;
    private PlantType? _draggedPlantType;
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _cellSize;

    public GameView(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _config = ConfigService.GetConfig();
        
        CalculateCellSize();
        InitializeGameField();
        SetupEventHandlers();
        StartRendering();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        
    }

    private void CalculateCellSize()
    {
        _cellSize = 120;
        _viewModel.CellSize = _cellSize;
    }

    private void InitializeGameField()
    {
        var rows = _config.Field.Rows;
        var cols = _config.Field.Columns;

        //BushLeftImage.Source = _viewModel.Session.Location.GetBushLeftImage();
        BushTopImage.Source = _viewModel.Session.Location.GetBushTopImage();
        BushRightImage.Source = _viewModel.Session.Location.GetBushRightImage();
        BushBottomImage.Source = _viewModel.Session.Location.GetBushBottomImage();
        
        GameField.Width = cols * _cellSize;
        GameField.Height = rows * _cellSize;
        Canvas.SetTop(GameField, _cellSize);
        Canvas.SetLeft(GameField, _cellSize);
    
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var rect = new FieldCell()
                {
                    AllowDrop = true,
                    Width = _cellSize,
                    Height = _cellSize,
                    Background = _viewModel.Session.Location.GetLocationCellColor(i, j),
                    Row = i,
                    Column = j,
                };
                
                GameField.Children.Add(rect);
                Canvas.SetLeft(rect, j * _cellSize);
                Canvas.SetTop(rect, i * _cellSize);
                
                rect.DragEnter += RectOnDragEnter;
                rect.DragLeave += RectOnDragLeave;
                rect.Drop += RectOnDrop;
            }
        }
    }

    private void RectOnDrop(object sender, DragEventArgs e)
    {
        Debug.WriteLine(nameof(RectOnDrop));
        if (e.Data.GetDataPresent(typeof(PlantType)) && sender is FieldCell fieldCell)
        {
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            fieldCell.PlacePlant(plantType);
            _viewModel.PlacePlant(plantType, fieldCell.Row, fieldCell.Column);
            HideDragPreview();
        }
    }

    private void RectOnDragLeave(object sender, DragEventArgs e)
    {
        Debug.WriteLine(nameof(RectOnDragLeave));
    }

    private void RectOnDragEnter(object sender, DragEventArgs e)
    {
        
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
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS for smooth animations
        };
        timer.Tick += (s, e) => 
        {
            RenderGame();
        };
        timer.Start();
    }

    private void RenderGame()
    {
        return;
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

            // Render plants
            foreach (var plant in _viewModel.Plants)
            {
                var rect = new Rectangle
                {
                    Width = _cellSize - 4,
                    Height = _cellSize - 4,
                    Fill = GetPlantColor(plant.Type),
                    Stroke = plant.State == PlantState.Active ? Brushes.Orange : Brushes.Black,
                    StrokeThickness = plant.State == PlantState.Active ? 3 : 2,
                    Tag = "Plant"
                };
                Canvas.SetLeft(rect, plant.Column * _cellSize + 2);
                Canvas.SetTop(rect, plant.Row * _cellSize + 2);
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
                Canvas.SetLeft(text, plant.Column * _cellSize + _cellSize / 2 - 15);
                Canvas.SetTop(text, plant.Row * _cellSize + _cellSize / 2 - 8);
                GameField.Children.Add(text);
            }

            // Render zombies
            foreach (var zombie in _viewModel.Zombies)
            {
                var rect = new Rectangle
                {
                    Width = _cellSize - 4,
                    Height = _cellSize - 4,
                    Fill = GetZombieColor(zombie.Type),
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Tag = "Zombie"
                };
                Canvas.SetLeft(rect, zombie.X);
                Canvas.SetTop(rect, zombie.Row * _cellSize + 2);
                GameField.Children.Add(rect);
            }

            // Render bullets
            foreach (var bullet in _viewModel.Bullets)
            {
                var bulletVisual = new Image()
                {
                    /*
                    Source = bullet.ParentPlantType switch
                    {
                        PlantType.Shooter1 => new BitmapImage(
                            new Uri("pack://application:,,,/Assets/Icons/shooter1_ball.png")),
                        PlantType.Shooter2 => new BitmapImage(
                            new Uri("pack://application:,,,/Assets/Icons/shooter2_ball.png")),
                        _ => throw new NotImplementedException(),
                    },
                    */
                    Width = 24,
                    Height = 24,
                };
                bulletVisual.StartSpinningAnimation();
                
                Canvas.SetLeft(bulletVisual, bullet.X - 5);
                Canvas.SetTop(bulletVisual, bullet.Row * _cellSize + _cellSize / 2d - 5);
                GameField.Children.Add(bulletVisual);
            }

            // Render suns with animations - use actual animated positions
            foreach (var sun in _viewModel.Suns)
            {
                var sunVisual = new Image()
                {
                    Source = new BitmapImage(
                        new Uri("pack://application:,,,/Assets/Icons/sun.png")),
                    Width = 48,
                    Tag = "Sun",
                    Cursor = Cursors.Hand,
                };
                
                // Use current animated position (centered)
                Canvas.SetLeft(sunVisual, sun.X - 15);
                Canvas.SetTop(sunVisual, sun.Y - 15);
                sunVisual.MouseLeftButtonDown += (s, e) =>
                {
                    var pos = e.GetPosition(GameField);
                    _viewModel.PickupSun(pos.X, pos.Y);
                };
                GameField.Children.Add(sunVisual);
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
                
                // Show drag preview
                ShowDragPreview(_draggedPlantType.Value, currentPoint);
                
                DragDrop.DoDragDrop(element, _draggedPlantType.Value, DragDropEffects.Move);
                
                // Hide drag preview
                HideDragPreview();
                
                element.ReleaseMouseCapture();
                _draggedPlantType = null;
                _isDragging = false;
            }
        }
    }
    
    private void ShowDragPreview(PlantType plantType, Point position)
    {
        DragPreview.Visibility = Visibility.Visible;
        var brush = GetPlantColor(plantType);
        if (brush is SolidColorBrush solidBrush)
        {
            DragPreview.Background = new SolidColorBrush(solidBrush.Color) { Opacity = 0.7 };
        }
        else
        {
            DragPreview.Background = new SolidColorBrush(Colors.Gray) { Opacity = 0.7 };
        }
        DragPreview.Width = _cellSize;
        DragPreview.Height = _cellSize;
        
        // Set preview text
        DragPreviewText.Text = plantType.ToString().Substring(0, Math.Min(3, plantType.ToString().Length));
        
        // Position near cursor initially
        UpdateDragPreviewPosition(plantType, position);
    }
    
    private void UpdateDragPreviewPosition(PlantType plantType, Point position)
    {
        if (DragPreview.Visibility == Visibility.Visible)
        {
            // Show which cell it will be placed in
            var fieldPos = Mouse.GetPosition(GameField);
            var row = (int)(fieldPos.Y / _cellSize);
            var col = (int)(fieldPos.X / _cellSize);
            
            if (row >= 0 && row < _config.Field.Rows && col >= 0 && col < _config.Field.Columns)
            {
                // Show preview at the cell position
                Canvas.SetLeft(DragPreview, col * _cellSize);
                Canvas.SetTop(DragPreview, row * _cellSize);
            }
            else
            {
                // Position near cursor if outside field
                Canvas.SetLeft(DragPreview, position.X - _cellSize / 2);
                Canvas.SetTop(DragPreview, position.Y - _cellSize / 2);
            }
        }
    }
    
    private void HideDragPreview()
    {
        DragPreview.Visibility = Visibility.Collapsed;
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

    private void GameField_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlantType)))
        {
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            var pos = e.GetPosition(this);
            ShowDragPreview(plantType, pos);
        }
    }

    private void GameField_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlantType)))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            
            // Update drag preview position
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            var pos = e.GetPosition(this);
            UpdateDragPreviewPosition(plantType, pos);
        }
    }

    private void GameField_DragLeave(object sender, DragEventArgs e)
    {
        HideDragPreview();
    }

    private void GameField_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlantType)))
        {
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            var pos = e.GetPosition(GameField);
            // Place on the cell at/below the cursor position
            var row = (int)(pos.Y / _cellSize);
            var col = (int)(pos.X / _cellSize);

            if (row >= 0 && row < _config.Field.Rows && col >= 0 && col < _config.Field.Columns)
            {
                _viewModel.PlacePlant(plantType, row, col);
            }
            
            HideDragPreview();
        }
    }
}
