namespace PlantsVsZombies.Models;

public class GameConfig
{
    public FieldConfig Field { get; set; } = new();
    public Dictionary<string, PlantConfig> Plants { get; set; } = new();
    public Dictionary<string, ZombieConfig> Zombies { get; set; } = new();
    public GameSettingsConfig Game { get; set; } = new();
}

public class FieldConfig
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int CellSize { get; set; }
}

public class PlantConfig
{
    public int Health { get; set; }
    public int Cost { get; set; }
    public double? ShootDelay { get; set; }
    public int? BulletsPerSpray { get; set; }
    public double? BulletSpeed { get; set; }
    public double? SunGenerationInterval { get; set; }
    public double? SunDropRange { get; set; }
    public int? SunValue { get; set; }
}

public class ZombieConfig
{
    public int Health { get; set; }
    public double Speed { get; set; }
    public int Damage { get; set; }
}

public class GameSettingsConfig
{
    public double ZombieSpawnMinDelay { get; set; }
    public double SunFallInterval { get; set; }
    public int SunValue { get; set; }
    public double SunPickupTimeout { get; set; }
    public double DifficultyIncreaseInterval { get; set; }
    public int InitialDifficulty { get; set; }
    public int MaxDifficulty { get; set; }
    public int ZombiesPerDifficulty { get; set; }
}
