using System.Windows;
using System.Windows.Controls;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class SignInView : UserControl
{
    public SignInViewModel ViewModel { get; }

    public SignInView()
    {
        InitializeComponent();
        ViewModel = new SignInViewModel();
        DataContext = ViewModel;
        
        // Handle password box since it doesn't support binding
        SignInCommandButton.Click += (s, e) =>
        {
            ViewModel.Password = PasswordBox.Password;
            ViewModel.SignInCommand.Execute(null);
        };
        
        Loaded += OnLoaded;
    }


    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
        //await Task.Delay(500);
       // ViewModel.Login = "lev";
       // ViewModel.Password = "lev";
       // ViewModel.SignInCommand.Execute(null);
#endif
    }
    
}
