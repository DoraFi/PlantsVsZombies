using System.Windows;
using PlantsVsZombies.Models;
using PlantsVsZombies.Models.Plant;
using PlantsVsZombies.Models.Zombie;
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
            SunBalance = ConfigService.GetConfig().Game.SunValue,
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

        //session.Plants.Add(plant);
    }

    public void UpdateGame(GameSession session, double currentTime, double deltaTime, double cellSize)
    {
        // Update score
        session.Score += deltaTime;
        
        IncreaseDifficulty(session, currentTime);
    }
    
    private void FallSunFromSky(GameSession session, double currentTime, double cellSize)
    {
        if (currentTime - session.LastSunFallTime >= _config.Game.SunFallInterval)
        {
            var sunX = _random.NextDouble() * (_config.Field.Columns * cellSize - 20);
            var targetY = 50 + _random.NextDouble() * (_config.Field.Rows * cellSize - 100);
            var sun = new Sun
            {
                StartX = sunX,
                StartY = 0,
                X = sunX,
                Y = 0,
                TargetX = sunX,
                TargetY = targetY,
                VelocityY = 0,
                SpawnTime = currentTime,
                IsFromGenerator = false,
                IsAnimating = true
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

    public bool PickupSun(GameSession session, Sun sun, double currentTime)
    {
        session.Suns.Remove(sun);
        session.SunBalance += _config.Game.SunValue;
        return true;
    }

    public PlantConfig GetPlantConfig(PlantType type)
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
