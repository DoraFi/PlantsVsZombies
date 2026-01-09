using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PlantsVsZombies.Helpers;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Plant;
using PlantsVsZombies.Models.Zombie;
using PlantsVsZombies.Services;
using PlantsVsZombies.ViewModels;
using PlantsVsZombies.VisualControls;
using Timer = System.Timers.Timer;

namespace PlantsVsZombies.Views;

public partial class GameView : UserControl
{
    private readonly GameViewModel _viewModel;
    private readonly GameConfig _config;
    private BasePlant? _draggedPlant;
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _cellSize;
    private int _rows;
    private int _columns;
    private System.Timers.Timer _sunSpawnTimer;
    private System.Timers.Timer _zombieSpawnTimer;
    private System.Timers.Timer _actionTimer;
    private List<FieldCell> _fieldCells = new();
    private Dictionary<BaseZombie, ZombieCell> _zombieCells = new();
    private Dictionary<Bullet, BulletCell> _bulletCells = new();
    private Dictionary<Sun, SunCell> _sunCells = new();

    public GameView(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _config = ConfigService.GetConfig();
        _viewModel.Paused += ViewModelOnPaused;
        _viewModel.Continued += ViewModelOnContinued;
        
        CalculateCellSize();
        InitializeGameField();
        SetupEventHandlers();
        StartRendering();
        InitializeSunFallTimer();
        InitializeZombieSpawnTimer();
        InitializeActionTimer();
    }

    private void InitializeActionTimer()
    {
        var fps = ConfigService.GetConfig().Game.FPS;
        _actionTimer = new Timer(TimeSpan.FromSeconds(1) / fps);
        _actionTimer.Start();
        _actionTimer.Elapsed += ActionTimerOnElapsed;
    }

    private void ActionTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        foreach (var zombie in _viewModel.Zombies.ToList())
        {
            zombie.MakeAction();
        }

        foreach (var bullet in _viewModel.Bullets.ToList())
        {
            bullet.MakeAction(_viewModel.Zombies);
        }
        
        foreach (var plant in _viewModel.Plants.ToList())
        {
            plant.MakeAction(_viewModel.Zombies);
        }
    }

    private void InitializeZombieSpawnTimer()
    {
        _zombieSpawnTimer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        _zombieSpawnTimer.Start();
       
        _zombieSpawnTimer.Elapsed += ZombieSpawnTimerOnElapsed;
    }

    private void ZombieSpawnTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        Random rnd = new Random();

        int randomValue = rnd.Next(0, 100);
        if (randomValue <= _viewModel.Session.Difficulty)
        {
            var row = rnd.Next(0, _rows);
            var zombieType = (ZombieType)rnd.Next(1, 3);
            SpawnZombie(row, zombieType);
        }
    }

    private void SpawnZombie(int row, ZombieType zombieType)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            BaseZombie baseZombie = zombieType switch
            {
                ZombieType.ZombieBoy => new ZombieBoy(_fieldCells.Where(cell => cell.Row == row).ToList(), _columns, row, _cellSize, _viewModel.Session.Location),
                ZombieType.ZombieGirl => new ZombieGirl(_fieldCells.Where(cell => cell.Row == row).ToList(),_columns, row, _cellSize, _viewModel.Session.Location),
                _ => throw new ArgumentOutOfRangeException(nameof(zombieType), zombieType, null)
            };
            
            _viewModel.Zombies.Add(baseZombie);

            // Create ZombieCell and set the zombie
            var zombieCell = new ZombieCell(_cellSize);
            zombieCell.SetZombie(baseZombie);

            // Store mapping for cleanup
            _zombieCells[baseZombie] = zombieCell;
            
            baseZombie.KillRequested += BaseZombieOnKillRequested; 
            
            GameField.Children.Add(zombieCell);
        });
    }

    private void BaseZombieOnKillRequested(BaseZombie obj)
    {
        try
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\levak\RiderProjects\DoraPlantsVsZombies\.cursor\debug.log", $"{{\"location\":\"GameView.cs:BaseZombieOnKillRequested\",\"message\":\"Removing zombie from visual tree\",\"zombieType\":\"{obj.GetType().Name}\",\"state\":\"{obj.State}\",\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            _viewModel.Zombies.Remove(obj);
            var cell = _zombieCells[obj];
            GameField.Children.Remove(cell);
            _zombieCells.Remove(obj);
        }
        catch (Exception ex)
        {
            
        }

    }

    private void InitializeSunFallTimer()
    {
        var timerInterval = ConfigService.GetConfig().Game.SunFallInterval;
        _sunSpawnTimer = new System.Timers.Timer(timerInterval);
        _sunSpawnTimer.Start();
        _sunSpawnTimer.Elapsed += SunSpawnTimerOnElapsed;
    }

    private void SunSpawnTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        
    }

    private void SummonSun(Point position)
    {
        
    }

    private void ViewModelOnPaused()
    {
        _sunSpawnTimer.Stop();
        _zombieSpawnTimer.Stop();
        _actionTimer.Stop();
    }
    
    private void ViewModelOnContinued()
    {
        _sunSpawnTimer.Start();
        _zombieSpawnTimer.Start();
        _actionTimer.Start();
    }

    private void CalculateCellSize()
    {
        _cellSize = 150;
        _viewModel.CellSize = _cellSize;
    }

    private void InitializeGameField()
    {
        _rows = _config.Field.Rows;
        _columns = _config.Field.Columns;

        //BushLeftImage.Source = _viewModel.Session.Location.GetBushLeftImage();
        BushTopImage.Source = _viewModel.Session.Location.GetBushTopImage();
        BushRightImage.Source = _viewModel.Session.Location.GetBushRightImage();
        BushBottomImage.Source = _viewModel.Session.Location.GetBushBottomImage();
        
        GameField.Width = _columns * _cellSize;
        GameField.Height = _rows * _cellSize;
        Canvas.SetTop(GameField, _cellSize);
        Canvas.SetLeft(GameField, _cellSize);
    
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                var fieldCell = new FieldCell()
                {
                    AllowDrop = true,
                    Width = _cellSize,
                    Height = _cellSize,
                    Background = _viewModel.Session.Location.GetLocationCellColor(i, j),
                    Row = i,
                    Column = j,
                };
                _fieldCells.Add(fieldCell);
                
                GameField.Children.Add(fieldCell);
                Canvas.SetLeft(fieldCell, j * _cellSize);
                Canvas.SetTop(fieldCell, i * _cellSize);
                
                fieldCell.DragEnter += RectOnDragEnter;
                fieldCell.DragLeave += RectOnDragLeave;
                fieldCell.Drop += RectOnDrop;
            }
        }
    }

    private void RectOnDrop(object sender, DragEventArgs e)
    {
        Debug.WriteLine(nameof(RectOnDrop));
        if (e.Data.GetDataPresent(typeof(PlantType)) && sender is FieldCell fieldCell)
        {
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            var plant = fieldCell.PlacePlant(plantType);
            
            _viewModel.Plants.Add(plant);
            plant.KillRequested += PlantOnKillRequested;
            plant.BulletSpawnRequested += PlantOnBulletSpawnRequested;
            if (plant is PlantGenerator plantGenerator)
            {
                plantGenerator.SunSpawnRequested += PlantGeneratorOnSunSpawnRequested;
            }
            
            _viewModel.PlacePlant(plantType, fieldCell.Row, fieldCell.Column);
            HideDragPreview();
        }
    }

    private void PlantGeneratorOnSunSpawnRequested(PlantGenerator obj)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var sun = new Sun()
            {
                X = obj.Column * _cellSize + _cellSize / 2,
                Y = obj.Row * _cellSize + _cellSize / 2,
            };

            var rnd = new Random();
            sun.X += rnd.Next(0, (int)_cellSize / 4);
            sun.Y += rnd.Next(0, (int)_cellSize / 4);
            
            _viewModel.Suns.Add(sun);

            var sunCell = new SunCell(sun);
            
            sunCell.MouseLeftButtonDown += (s, e) =>
            {
                var pos = e.GetPosition(GameField);
                _viewModel.PickupSun(pos.X, pos.Y);
                GameField.Children.Remove(sunCell);
                _sunCells.Remove(sun);
            };
            
            Canvas.SetTop(sunCell, sun.Y);
            Canvas.SetLeft(sunCell, sun.X);
            _viewModel.Suns.Add(sun);
            GameField.Children.Add(sunCell);
            _sunCells.Add(sun, sunCell);
        });
    }

    private void PlantOnKillRequested(BasePlant plant)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _viewModel.Plants.Remove(plant);
        });
    }

    private void PlantOnBulletSpawnRequested(BasePlant plant)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var bullet = new Bullet()
            {
                ParentPlantType = plant.Type,
                Row = plant.Row,
                X = plant.Column * _cellSize
            };
            bullet.KillRequested += BulletOnKillRequested;
            _viewModel.Bullets.Add(bullet);

            var bulletCell = new BulletCell(bullet);
            Canvas.SetTop(bulletCell, plant.Row * _cellSize + _cellSize * 0.25);
            GameField.Children.Add(bulletCell);

            _bulletCells.Add(bullet, bulletCell);
        });
    }

    private void BulletOnKillRequested(Bullet obj)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Bullets.Remove(obj);
                var cell = _bulletCells[obj];
                GameField.Children.Remove(cell);
                _bulletCells.Remove(obj);
            });

        }
        catch (Exception ex)
        {
            
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
        _viewModel.Zombies.CollectionChanged += (s, e) =>
        {
            // Remove ZombieCells when zombies are removed
            if (e.OldItems != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (BaseZombie zombie in e.OldItems)
                    {
                        if (_zombieCells.TryGetValue(zombie, out var zombieCell))
                        {
                            GameField.Children.Remove(zombieCell);
                            _zombieCells.Remove(zombie);
                        }
                    }
                });
            }
            RenderGame();
        };
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
        return;
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
            ZombieType.ZombieBoy => Brushes.Pink,
            ZombieType.ZombieGirl => Brushes.Blue,
            _ => Brushes.Gray
        };
    }

    private void PlantShopItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is BasePlant plant)
        {
            if (!_viewModel.CanAffordPlant(plant.Type))
                return;

            _draggedPlant = plant;
            _isDragging = false;
            _dragStartPoint = e.GetPosition(this);
            element.CaptureMouse();
        }
    }

    private void PlantShopItem_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.IsMouseCaptured && _draggedPlant != null)
        {
            var currentPoint = e.GetPosition(this);
            var distance = Math.Sqrt(Math.Pow(currentPoint.X - _dragStartPoint.X, 2) + 
                                   Math.Pow(currentPoint.Y - _dragStartPoint.Y, 2));
            
            Debug.WriteLine(DateTime.Now.ToString() + nameof(PlantShopItem_MouseMove));
            
            if (distance > 5) // Start dragging after 5 pixels
            {
                _isDragging = true;
                
                // Show drag preview
                Debug.WriteLine(DateTime.Now.ToString() + nameof(ShowDragPreview));
                ShowDragPreview(_draggedPlant, currentPoint);
              
                DragDrop.DoDragDrop(element, _draggedPlant.Type, DragDropEffects.Move);
                
                // Hide drag preview
                Debug.WriteLine(DateTime.Now.ToString() + nameof(HideDragPreview));
                HideDragPreview();
                
                element.ReleaseMouseCapture();
                //_draggedPlantType = null;
                _isDragging = false;
            }
        }
    }

    private void ShowDragPreview(BasePlant basePlant, Point position)
    {
        DragPreview.Visibility = Visibility.Visible;
        
        DragPreview.Width = _cellSize;
        DragPreview.Height = _cellSize;
        DragPreview.PlacePlant(basePlant.Type);
        
        // Position near cursor initially
        UpdateDragPreviewPosition(position);
    }
    
    private void UpdateDragPreviewPosition(Point position)
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
            _draggedPlant = null;
            _isDragging = false;
        }
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
