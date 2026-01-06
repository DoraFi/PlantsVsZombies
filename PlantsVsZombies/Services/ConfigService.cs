using System.IO;
using Newtonsoft.Json;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Services;

public class ConfigService
{
    private static GameConfig? _config;
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "Config", 
        "gameconfig.json");

    public static GameConfig GetConfig()
    {
        if (_config != null)
            return _config;

        // Try multiple paths
        var paths = new[]
        {
            ConfigPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Config", "gameconfig.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PlantsVsZombies", "Config", "gameconfig.json")
        };

        string? configContent = null;
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                configContent = File.ReadAllText(path);
                break;
            }
        }

        if (configContent == null)
            throw new FileNotFoundException($"Config file not found. Tried: {string.Join(", ", paths)}");

        _config = JsonConvert.DeserializeObject<GameConfig>(configContent) 
            ?? throw new InvalidOperationException("Failed to deserialize config");
        return _config;
    }
}
