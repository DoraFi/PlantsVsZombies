using System.Windows.Controls;
using PlantsVsZombies.ViewModels;

namespace PlantsVsZombies.Views;

public partial class SignUpView : UserControl
{
    public SignUpViewModel ViewModel { get; }

    public SignUpView()
    {
        InitializeComponent();
        ViewModel = new SignUpViewModel();
        DataContext = ViewModel;
        
        // Handle password box since it doesn't support binding
        SignUpCommandButton.Click += (s, e) =>
        {
            ViewModel.Password = PasswordBox.Password;
            ViewModel.SignUpCommand.Execute(null);
        };
    }
}
