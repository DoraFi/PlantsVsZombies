using System.IO;
using System.Xml.Serialization;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Services;

public class ConfigService
{
    private static GameConfig? _config;
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "Config", 
        "gameconfig.xml");

    public static GameConfig GetConfig()
    {
        if (_config != null)
            return _config;

        // Try multiple paths
        var paths = new[]
        {
            ConfigPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Config", "gameconfig.xml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PlantsVsZombies", "Config", "gameconfig.xml")
        };

        string? configFilePath = null;
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                configFilePath = path;
                break;
            }
        }

        if (configFilePath == null)
            throw new FileNotFoundException($"Config file not found. Tried: {string.Join(", ", paths)}");

        var serializer = new XmlSerializer(typeof(GameConfig));
        using var reader = new StreamReader(configFilePath);
        _config = (GameConfig?)serializer.Deserialize(reader) 
            ?? throw new InvalidOperationException("Failed to deserialize config");
        
        return _config;
    }
}
