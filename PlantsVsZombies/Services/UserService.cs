using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.Services;

public class UserService
{
    private static readonly string UsersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
    private static List<User>? _users;
    private static User? _currentUser;

    public static User? CurrentUser => _currentUser;

    private static List<User> LoadUsers()
    {
        if (_users != null)
            return _users;

        if (!File.Exists(UsersPath))
        {
            _users = new List<User>();
            SaveUsers();
            return _users;
        }

        try
        {
            var json = File.ReadAllText(UsersPath);
            _users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }
        catch
        {
            _users = new List<User>();
        }
        return _users;
    }

    private static void SaveUsers()
    {
        try
        {
            var directory = Path.GetDirectoryName(UsersPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_users, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(UsersPath, json);
        }
        catch (Exception ex)
        {
            // Ignore save errors
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool SignUp(string login, string password)
    {
        var users = LoadUsers();
        if (users.Any(u => u.Login == login))
            return false;

        var user = new User
        {
            Login = login,
            Password = HashPassword(password),
            TopScores = new List<double>()
        };

        users.Add(user);
        SaveUsers();
        return true;
    }

    public static bool SignIn(string login, string password)
    {
        var users = LoadUsers();
        var hashedPassword = HashPassword(password);
        var user = users.FirstOrDefault(u => u.Login == login && u.Password == hashedPassword);

        if (user == null)
            return false;

        _currentUser = user;
        return true;
    }

    public static void SignOut()
    {
        _currentUser = null;
    }

    public static void UpdateUserScore(double score)
    {
        if (_currentUser == null)
            return;

        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Login == _currentUser.Login);
        if (user == null)
            return;

        user.TopScores.Add(score);
        user.TopScores = user.TopScores.OrderByDescending(s => s).Take(3).ToList();
        SaveUsers();
        _currentUser = user;
    }

    public static void SaveGameSession(GameSession session)
    {
        if (_currentUser == null)
            return;

        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Login == _currentUser.Login);
        if (user == null)
            return;

        session.SyncFromObservable();
        user.ActiveSession = session;
        SaveUsers();
        _currentUser = user;
    }

    public static GameSession? LoadGameSession()
    {
        if (_currentUser?.ActiveSession == null)
            return null;

        var session = _currentUser.ActiveSession;
        session.SyncToObservable();
        return session;
    }

    public static void ClearGameSession()
    {
        if (_currentUser == null)
            return;

        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Login == _currentUser.Login);
        if (user == null)
            return;

        user.ActiveSession = null;
        SaveUsers();
        _currentUser = user;
    }
}
