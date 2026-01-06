using System.Windows;
using PlantsVsZombies.Models;
using PlantsVsZombies.Services;

namespace PlantsVsZombies.Services;

public class GameService
{
    private readonly GameConfig _config;
    private readonly Random _random = new();

    public GameService()
    {
        _config = ConfigService.GetConfig();
    }

    public GameSession CreateNewGame(LocationType location, int initialDifficulty)
    {
        var session = new GameSession
        {
            Location = location,
            Difficulty = initialDifficulty,
            Score = 0,
            SunBalance = 50,
            LastZombieSpawnTime = 0,
            LastSunFallTime = 0,
            LastDifficultyIncreaseTime = 0,
            LastZombieSpawnTimeByRow = new Dictionary<int, double>()
        };
        session.SyncToObservable();
        return session;
    }

    public bool CanPlacePlant(GameSession session, int row, int column, PlantType plantType)
    {
        if (row < 0 || row >= _config.Field.Rows || column < 0 || column >= _config.Field.Columns)
            return false;

        if (session.Plants.Any(p => p.Row == row && p.Column == column))
            return false;

        var plantConfig = GetPlantConfig(plantType);
        return session.SunBalance >= plantConfig.Cost;
    }

    public bool CanAffordPlant(GameSession session, PlantType plantType)
    {
        var plantConfig = GetPlantConfig(plantType);
        return session.SunBalance >= plantConfig.Cost;
    }

    public void PlacePlant(GameSession session, int row, int column, PlantType plantType)
    {
        if (!CanPlacePlant(session, row, column, plantType))
            return;

        var plantConfig = GetPlantConfig(plantType);
        session.SunBalance -= plantConfig.Cost;

        var plant = new Plant
        {
            Type = plantType,
            Row = row,
            Column = column,
            Health = plantConfig.Health,
            State = PlantState.Idle,
            LastShootTime = 0,
            LastSunGenerationTime = 0
        };

        session.Plants.Add(plant);
    }

    public void UpdateGame(GameSession session, double currentTime, double deltaTime)
    {
        // Update score
        session.Score += deltaTime;

        // Spawn zombies
        SpawnZombies(session, currentTime);

        // Update zombies
        UpdateZombies(session, currentTime, deltaTime);

        // Update plants (shooting, sun generation)
        UpdatePlants(session, currentTime, deltaTime);

        // Update bullets
        UpdateBullets(session, deltaTime);

        // Update suns
        UpdateSuns(session, currentTime, deltaTime);

        // Check for game over
        CheckGameOver(session);

        // Increase difficulty
        IncreaseDifficulty(session, currentTime);

        // Fall sun from sky
        FallSunFromSky(session, currentTime);
    }

    private void SpawnZombies(GameSession session, double currentTime)
    {
        var spawnRate = _config.Game.ZombiesPerDifficulty * session.Difficulty / _config.Game.DifficultyIncreaseInterval;
        var spawnInterval = 1.0 / spawnRate;

        if (currentTime - session.LastZombieSpawnTime < spawnInterval)
            return;

        var availableRows = new List<int>();
        for (int i = 0; i < _config.Field.Rows; i++)
        {
            if (!session.LastZombieSpawnTimeByRow.ContainsKey(i) ||
                currentTime - session.LastZombieSpawnTimeByRow[i] >= _config.Game.ZombieSpawnMinDelay)
            {
                availableRows.Add(i);
            }
        }

        if (availableRows.Count == 0)
            return;

        var row = availableRows[_random.Next(availableRows.Count)];
        var zombieType = _random.Next(2) == 0 ? ZombieType.GirlZombie : ZombieType.BoyZombie;
        var zombieConfig = GetZombieConfig(zombieType);

        var zombie = new Zombie
        {
            Type = zombieType,
            Row = row,
            X = _config.Field.Columns * _config.Field.CellSize,
            Health = zombieConfig.Health,
            LastDamageTime = 0
        };

        session.Zombies.Add(zombie);
        session.LastZombieSpawnTime = currentTime;
        session.LastZombieSpawnTimeByRow[row] = currentTime;
    }

    private void UpdateZombies(GameSession session, double currentTime, double deltaTime)
    {
        var zombiesToRemove = new List<Zombie>();

        foreach (var zombie in session.Zombies)
        {
            var zombieConfig = GetZombieConfig(zombie.Type);
            zombie.X -= zombieConfig.Speed * deltaTime;

            // Check collision with plants
            var plantInRow = session.Plants
                .Where(p => p.Row == zombie.Row && p.Health > 0)
                .OrderBy(p => p.Column)
                .FirstOrDefault();

            if (plantInRow != null)
            {
                var plantX = plantInRow.Column * _config.Field.CellSize;
                if (zombie.X <= plantX + _config.Field.CellSize && zombie.X >= plantX)
                {
                    // Zombie is eating plant
                    if (currentTime - zombie.LastDamageTime >= 1.0)
                    {
                        plantInRow.Health -= zombieConfig.Damage;
                        zombie.LastDamageTime = currentTime;

                        if (plantInRow.Type == PlantType.Shield)
                        {
                            plantInRow.State = PlantState.Active;
                        }

                        if (plantInRow.Health <= 0)
                        {
                            session.Plants.Remove(plantInRow);
                        }
                    }
                }
            }

            // Check if zombie reached left edge
            if (zombie.X <= 0)
            {
                // Game over
                session.Score = -1; // Mark as game over
            }

            // Remove dead zombies
            if (zombie.Health <= 0)
            {
                zombiesToRemove.Add(zombie);
            }
        }

        foreach (var zombie in zombiesToRemove)
        {
            session.Zombies.Remove(zombie);
        }
    }

    private void UpdatePlants(GameSession session, double currentTime, double deltaTime)
    {
        foreach (var plant in session.Plants.ToList())
        {
            if (plant.Health <= 0)
            {
                session.Plants.Remove(plant);
                continue;
            }

            var plantConfig = GetPlantConfig(plant.Type);

            // Handle shooting plants
            if (plant.Type == PlantType.Shooter1 || plant.Type == PlantType.Shooter2)
            {
                if (plantConfig.ShootDelay.HasValue && currentTime - plant.LastShootTime >= plantConfig.ShootDelay.Value)
                {
                    // Check if there's a zombie in the row (to the right of the plant)
                    var plantX = plant.Column * _config.Field.CellSize;
                    var zombieInRow = session.Zombies
                        .FirstOrDefault(z => z.Row == plant.Row && z.X >= plantX && z.Health > 0);

                    if (zombieInRow != null)
                    {
                        plant.State = PlantState.Active;
                        plant.LastShootTime = currentTime;

                        // Shoot bullets
                        var bulletsPerSpray = plantConfig.BulletsPerSpray ?? 1;
                        for (int i = 0; i < bulletsPerSpray; i++)
                        {
                            var bullet = new Bullet
                            {
                                Row = plant.Row,
                                X = plant.Column * _config.Field.CellSize + _config.Field.CellSize / 2
                            };
                            session.Bullets.Add(bullet);
                        }

                        // Reset state after animation (simplified - in real game would use animation duration)
                        Task.Delay(200).ContinueWith(_ =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (session.Plants.Contains(plant))
                                    plant.State = PlantState.Idle;
                            });
                        });
                    }
                }
            }

            // Handle generator plants
            if (plant.Type == PlantType.Generator)
            {
                if (plantConfig.SunGenerationInterval.HasValue &&
                    currentTime - plant.LastSunGenerationTime >= plantConfig.SunGenerationInterval.Value)
                {
                    plant.LastSunGenerationTime = currentTime;

                    // Generate sun around generator
                    var plantX = plant.Column * _config.Field.CellSize;
                    var plantY = plant.Row * _config.Field.CellSize;
                    var range = (plantConfig.SunDropRange ?? 0.5) * _config.Field.CellSize;

                    var sunX = Math.Max(0, Math.Min(_config.Field.Columns * _config.Field.CellSize - 20,
                        plantX + (_random.NextDouble() - 0.5) * range * 2));
                    var sunY = Math.Max(0, Math.Min(_config.Field.Rows * _config.Field.CellSize - 20,
                        plantY + (_random.NextDouble() - 0.5) * range * 2));

                    // Start sun at generator position, animate to target
                    var sun = new Sun
                    {
                        StartX = plantX + _config.Field.CellSize / 2,
                        StartY = plantY + _config.Field.CellSize / 2,
                        X = plantX + _config.Field.CellSize / 2,
                        Y = plantY + _config.Field.CellSize / 2,
                        TargetX = sunX,
                        TargetY = sunY,
                        VelocityY = 0,
                        SpawnTime = currentTime,
                        IsFromGenerator = true,
                        IsAnimating = true
                    };
                    session.Suns.Add(sun);
                }
            }
        }
    }

    private void UpdateBullets(GameSession session, double deltaTime)
    {
        var bulletsToRemove = new List<Bullet>();
        var bulletSpeed = _config.Plants["Shooter1"].BulletSpeed ?? 500;

        foreach (var bullet in session.Bullets)
        {
            bullet.X += bulletSpeed * deltaTime;

            // Check collision with zombies
            var zombieHit = session.Zombies.FirstOrDefault(z =>
                z.Row == bullet.Row &&
                z.X <= bullet.X &&
                z.X + _config.Field.CellSize >= bullet.X);

            if (zombieHit != null)
            {
                zombieHit.Health -= 50; // Bullet damage
                bulletsToRemove.Add(bullet);
                continue;
            }

            // Check if bullet reached right edge
            if (bullet.X >= _config.Field.Columns * _config.Field.CellSize)
            {
                bulletsToRemove.Add(bullet);
            }
        }

        foreach (var bullet in bulletsToRemove)
        {
            session.Bullets.Remove(bullet);
        }
    }

    private void UpdateSuns(GameSession session, double currentTime, double deltaTime)
    {
        var sunsToRemove = new List<Sun>();
        const double gravity = 300.0; // pixels per second squared
        const double bounceDamping = 0.3;

        foreach (var sun in session.Suns)
        {
            if (currentTime - sun.SpawnTime >= _config.Game.SunPickupTimeout)
            {
                sunsToRemove.Add(sun);
                continue;
            }

            // Animate sun falling or popping
            if (sun.IsAnimating)
            {
                if (sun.IsFromGenerator)
                {
                    // Pop animation from generator - arc trajectory
                    var elapsed = currentTime - sun.SpawnTime;
                    var duration = 0.5; // 0.5 seconds for pop animation
                    
                    if (elapsed < duration)
                    {
                        var t = elapsed / duration;
                        // Arc trajectory: start at generator, arc to target
                        var arcHeight = 30.0; // Height of arc
                        sun.X = sun.StartX + (sun.TargetX - sun.StartX) * t; // Interpolate X
                        sun.Y = sun.StartY - arcHeight * (4 * t * (1 - t)); // Arc motion
                    }
                    else
                    {
                        // After arc, fall to target position
                        sun.VelocityY += gravity * deltaTime;
                        sun.Y += sun.VelocityY * deltaTime;
                        
                        if (sun.Y >= sun.TargetY)
                        {
                            sun.Y = sun.TargetY;
                            sun.VelocityY = 0;
                            sun.IsAnimating = false;
                        }
                    }
                }
                else
                {
                    // Falling from sky animation
                    sun.VelocityY += gravity * deltaTime;
                    sun.Y += sun.VelocityY * deltaTime;
                    
                    if (sun.Y >= sun.TargetY)
                    {
                        sun.Y = sun.TargetY;
                        sun.VelocityY *= -bounceDamping; // Bounce
                        if (Math.Abs(sun.VelocityY) < 10)
                        {
                            sun.VelocityY = 0;
                            sun.IsAnimating = false;
                        }
                    }
                }
            }
        }

        foreach (var sun in sunsToRemove)
        {
            session.Suns.Remove(sun);
        }
    }

    private void FallSunFromSky(GameSession session, double currentTime)
    {
        if (currentTime - session.LastSunFallTime >= _config.Game.SunFallInterval)
        {
            var sunX = _random.NextDouble() * (_config.Field.Columns * _config.Field.CellSize - 20);
            var sun = new Sun
            {
                X = sunX,
                Y = 0,
                SpawnTime = currentTime,
                IsFromGenerator = false
            };
            session.Suns.Add(sun);
            session.LastSunFallTime = currentTime;
        }
    }

    private void IncreaseDifficulty(GameSession session, double currentTime)
    {
        if (currentTime - session.LastDifficultyIncreaseTime >= _config.Game.DifficultyIncreaseInterval)
        {
            if (session.Difficulty < _config.Game.MaxDifficulty)
            {
                session.Difficulty++;
            }
            session.LastDifficultyIncreaseTime = currentTime;
        }
    }

    private void CheckGameOver(GameSession session)
    {
        if (session.Zombies.Any(z => z.X <= 0))
        {
            session.Score = -1; // Game over marker
        }
    }

    public bool PickupSun(GameSession session, double x, double y, double currentTime)
    {
        var sun = session.Suns.FirstOrDefault(s =>
            Math.Abs(s.X - x) < 30 && Math.Abs(s.Y - y) < 30 &&
            currentTime - s.SpawnTime < _config.Game.SunPickupTimeout);

        if (sun != null)
        {
            session.Suns.Remove(sun);
            session.SunBalance += _config.Game.SunValue;
            return true;
        }

        return false;
    }

    private PlantConfig GetPlantConfig(PlantType type)
    {
        var key = type.ToString();
        return _config.Plants[key];
    }

    private ZombieConfig GetZombieConfig(ZombieType type)
    {
        var key = type.ToString();
        return _config.Zombies[key];
    }
}
