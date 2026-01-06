using PlantsVsZombies.Views;

namespace PlantsVsZombies.ViewModels;

public class NavigationService
{
    private static NavigationService? _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();

    public event Action<BaseViewModel>? NavigationRequested;

    public void NavigateTo(BaseViewModel viewModel)
    {
        NavigationRequested?.Invoke(viewModel);
    }
}
