using System.Linq;
using System.Xml.Serialization;

namespace PlantsVsZombies.Models;

[XmlRoot("GameConfig")]
public class GameConfig
{
    [XmlElement("Field")]
    public FieldConfig Field { get; set; } = new();
    
    [XmlArray("Plants")]
    [XmlArrayItem("PlantConfig")]
    public List<PlantConfigEntry> PlantsList { get; set; } = new();
    
    [XmlIgnore]
    public Dictionary<string, PlantConfig> Plants
    {
        get => PlantsList?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, PlantConfig>();
        set => PlantsList = value?.Select(kvp => new PlantConfigEntry { Key = kvp.Key, Value = kvp.Value }).ToList() ?? new List<PlantConfigEntry>();
    }
    
    [XmlArray("Zombies")]
    [XmlArrayItem("ZombieConfig")]
    public List<ZombieConfigEntry> ZombiesList { get; set; } = new();
    
    [XmlIgnore]
    public Dictionary<string, ZombieConfig> Zombies
    {
        get => ZombiesList?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, ZombieConfig>();
        set => ZombiesList = value?.Select(kvp => new ZombieConfigEntry { Key = kvp.Key, Value = kvp.Value }).ToList() ?? new List<ZombieConfigEntry>();
    }
    
    [XmlElement("Game")]
    public GameSettingsConfig Game { get; set; } = new();
}

[XmlType("PlantConfig")]
public class PlantConfigEntry
{
    [XmlElement("Key")]
    public string Key { get; set; } = string.Empty;
    
    [XmlElement("Value")]
    public PlantConfig Value { get; set; } = new();
}

[XmlType("ZombieConfig")]
public class ZombieConfigEntry
{
    [XmlElement("Key")]
    public string Key { get; set; } = string.Empty;
    
    [XmlElement("Value")]
    public ZombieConfig Value { get; set; } = new();
}

[XmlType("Field")]
public class FieldConfig
{
    [XmlElement("Rows")]
    public int Rows { get; set; }
    
    [XmlElement("Columns")]
    public int Columns { get; set; }
}

[XmlType("PlantConfigData")]
public class PlantConfig
{
    [XmlElement("Health")]
    public int Health { get; set; }
    
    [XmlElement("Cost")]
    public int Cost { get; set; }
    
    [XmlElement("ShootDelay")]
    public double ShootDelay { get; set; }
    
    [XmlElement("BulletsPerSpray")]
    public int BulletsPerSpray { get; set; }
    
    [XmlElement("BulletSpeed")]
    public double BulletSpeed { get; set; }
    
    [XmlElement("BulletDamage")]
    public double BulletDamage { get; set; }
    
    [XmlElement("SunGenerationInterval")]
    public double? SunGenerationInterval { get; set; }
    
    [XmlElement("SunDropRange")]
    public double? SunDropRange { get; set; }
    
    [XmlElement("SunValue")]
    public int? SunValue { get; set; }
    
    [XmlIgnore]
    public bool SunGenerationIntervalSpecified => SunGenerationInterval.HasValue;
    
    [XmlIgnore]
    public bool SunDropRangeSpecified => SunDropRange.HasValue;
    
    [XmlIgnore]
    public bool SunValueSpecified => SunValue.HasValue;
}

[XmlType("ZombieConfigData")]
public class ZombieConfig
{
    [XmlElement("Health")]
    public int Health { get; set; }
    
    [XmlElement("Speed")]
    public double Speed { get; set; }
    
    [XmlElement("Damage")]
    public int Damage { get; set; }
}

[XmlType("Game")]
public class GameSettingsConfig
{
    [XmlElement("ZombieSpawnMinDelay")]
    public double ZombieSpawnMinDelay { get; set; }
    
    [XmlElement("SunFallInterval")]
    public double SunFallInterval { get; set; }
    
    [XmlElement("SunValue")]
    public int SunValue { get; set; }
    
    [XmlElement("SunPickupTimeout")]
    public double SunPickupTimeout { get; set; }
    
    [XmlElement("DifficultyIncreaseInterval")]
    public double DifficultyIncreaseInterval { get; set; }
    
    [XmlElement("InitialDifficulty")]
    public int InitialDifficulty { get; set; }
    
    [XmlElement("MaxDifficulty")]
    public int MaxDifficulty { get; set; }
    
    [XmlElement("ZombiesPerDifficulty")]
    public int ZombiesPerDifficulty { get; set; }
    
    [XmlElement("FPS")]
    public int FPS { get; set; }
}
