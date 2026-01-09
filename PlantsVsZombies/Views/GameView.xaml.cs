using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        InitializeSunFallTimer();
        InitializeZombieSpawnTimer();
        InitializeActionTimer();
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var zombie in _viewModel.Session.Zombies.ToList())
            {
                SpawnZombie(zombie);
            }
        }
        catch {}
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
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BaseZombie baseZombie = zombieType switch
                {
                    ZombieType.ZombieBoy => new ZombieBoy(_fieldCells.Where(cell => cell.Row == row).ToList(), _columns,
                        row, _cellSize, _viewModel.Session.Location),
                    ZombieType.ZombieGirl => new ZombieGirl(_fieldCells.Where(cell => cell.Row == row).ToList(),
                        _columns, row, _cellSize, _viewModel.Session.Location),
                    _ => throw new ArgumentOutOfRangeException(nameof(zombieType), zombieType, null)
                };
                SpawnZombie(baseZombie);

            });
        }
        catch (Exception ex)
        {
            
        }
    }
    
    private void SpawnZombie(BaseZombie baseZombie) 
    {
        _viewModel.Zombies.Add(baseZombie);

        // Create ZombieCell and set the zombie
        var zombieCell = new ZombieCell(_cellSize);
        zombieCell.SetZombie(baseZombie);

        // Store mapping for cleanup
        _zombieCells[baseZombie] = zombieCell;

        baseZombie.KillRequested += BaseZombieOnKillRequested;

        GameField.Children.Add(zombieCell);
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
        _cellSize = 120;
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
            fieldCell.Opacity = 1;
            
            var plantType = (PlantType)e.Data.GetData(typeof(PlantType));
            
            // Place the plant (this checks affordability and deducts the cost)
            // We need to do this BEFORE adding the plant to the collection, otherwise
            // CanPlacePlant will return false because it sees a plant already at that position
            if (!_viewModel.CanPlacePlant(plantType, fieldCell.Row, fieldCell.Column))
            {
                HideDragPreview();
                return;
            }
            
            // Deduct the cost by calling PlacePlant BEFORE adding to collection
            _viewModel.PlacePlant(plantType, fieldCell.Row, fieldCell.Column);
            
            // Now create and add the plant to the collection
            var plant = fieldCell.PlacePlant(plantType);
            _viewModel.Plants.Add(plant);
            plant.KillRequested += PlantOnKillRequested;
            plant.BulletSpawnRequested += PlantOnBulletSpawnRequested;
            if (plant is PlantGenerator plantGenerator)
            {
                plantGenerator.SunSpawnRequested += PlantGeneratorOnSunSpawnRequested;
            }
            
            HideDragPreview();
        }
    }

    private void PlantGeneratorOnSunSpawnRequested(PlantGenerator obj)
    {
        double x = obj.Column * _cellSize + _cellSize / 2;
        double y = obj.Row * _cellSize + _cellSize / 2;
        SpawnSun(x, y);
    }

    private void SpawnSun(double x, double y)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var sun = new Sun()
            {
                X = x, Y = y
            };

            var rnd = new Random();
            sun.X += rnd.Next(0, (int)_cellSize / 4);
            sun.Y += rnd.Next(0, (int)_cellSize / 4);
            
            _viewModel.Suns.Add(sun);

            var sunCell = new SunCell(sun);
            
            sunCell.MouseLeftButtonDown += (s, e) =>
            {
                var pos = e.GetPosition(GameField);
                _viewModel.PickupSun(sun);
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
                X = plant.Column * _cellSize + _cellSize / 2,
            };
            bullet.KillRequested += BulletOnKillRequested;
            _viewModel.Bullets.Add(bullet);

            var bulletCell = new BulletCell(bullet);
            Canvas.SetTop(bulletCell, plant.Row * _cellSize + _cellSize * 0.25);
            GameField.Children.Add(bulletCell);

            _bulletCells.Add(bullet, bulletCell);
        });
    }

    public void SpawnBullet(Bullet bullet)
    {
        
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
        if (sender is FieldCell fieldCell)
        {
            fieldCell.Opacity = 1;
        }
    }

    private void RectOnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is FieldCell fieldCell)
        {
            fieldCell.Opacity = 0.75;
        }
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
                
                DragDrop.DoDragDrop(element, _draggedPlant.Type, DragDropEffects.Move);
                
                // Hide drag preview
                HideDragPreview();
                
                element.ReleaseMouseCapture();
                //_draggedPlantType = null;
                _isDragging = false;
            }
        }
    }
    
    
    
    private void HideDragPreview()
    {
        
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
