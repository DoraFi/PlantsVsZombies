using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlantsVsZombies.Models;

public partial class Sun : ObservableObject
{
    [ObservableProperty]
    private double _x;
    
    [ObservableProperty]
    private double _y;
    
    public double SpawnTime { get; set; }
    public bool IsFromGenerator { get; set; }
    public double TargetY { get; set; }
    public double TargetX { get; set; }
    public double VelocityY { get; set; }
    public double StartX { get; set; }
    public double StartY { get; set; }
    public bool IsAnimating { get; set; }
}
