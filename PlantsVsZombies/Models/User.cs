namespace PlantsVsZombies.Models;

public class User
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<double> TopScores { get; set; } = new();
    public GameSession? ActiveSession { get; set; }
}
