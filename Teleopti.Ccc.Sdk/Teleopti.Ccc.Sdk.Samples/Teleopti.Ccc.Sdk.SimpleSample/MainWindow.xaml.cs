using System.Windows;
using Teleopti.Ccc.Sdk.SimpleSample.ViewModel;

namespace Teleopti.Ccc.Sdk.SimpleSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new SimpleSampleViewModel();
        }
    }
}
