using System.Windows;
using System.Windows.Controls;

namespace PlantsVsZombies.VisualControls
{
    public partial class CancelButton : UserControl
    {
        public CancelButton()
        {
            InitializeComponent();
        }

        private void CancelAppButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}