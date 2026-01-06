using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace PlantsVsZombies.Models;

public partial class GameSession : ObservableObject
{
    [ObservableProperty] private LocationType _location;

    [ObservableProperty] private int _difficulty;

    [ObservableProperty] private double _score;

    [ObservableProperty] private int _sunBalance;
    
    [JsonIgnore]
    public ObservableCollection<Plant> Plants { get; set; } = new();
    
    [JsonIgnore]
    public ObservableCollection<Zombie> Zombies { get; set; } = new();
    
    [JsonIgnore]
    public ObservableCollection<Bullet> Bullets { get; set; } = new();
    
    [JsonIgnore]
    public ObservableCollection<Sun> Suns { get; set; } = new();
    
    // For serialization
    public List<Plant> PlantsList { get; set; } = new();
    public List<Zombie> ZombiesList { get; set; } = new();
    public List<Bullet> BulletsList { get; set; } = new();
    public List<Sun> SunsList { get; set; } = new();
    
    public double LastZombieSpawnTime { get; set; }
    public double LastSunFallTime { get; set; }
    public double LastDifficultyIncreaseTime { get; set; }
    public Dictionary<int, double> LastZombieSpawnTimeByRow { get; set; } = new();
    
    public void SyncToObservable()
    {
        Plants.Clear();
        foreach (var plant in PlantsList)
            Plants.Add(plant);
        
        Zombies.Clear();
        foreach (var zombie in ZombiesList)
            Zombies.Add(zombie);
        
        Bullets.Clear();
        foreach (var bullet in BulletsList)
            Bullets.Add(bullet);
        
        Suns.Clear();
        foreach (var sun in SunsList)
            Suns.Add(sun);
    }
    
    public void SyncFromObservable()
    {
        PlantsList = Plants.ToList();
        ZombiesList = Zombies.ToList();
        BulletsList = Bullets.ToList();
        SunsList = Suns.ToList();
    }
}
