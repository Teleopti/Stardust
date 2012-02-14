using System.Windows;
using Syncfusion.Windows.Shared;

namespace Teleopti.Ccc.WpfControls.Common
{
    /// <summary>
    /// Interaction logic for OkCancelWindow.xaml
    /// </summary>
    public partial class OkCancelWindow : ChromelessWindow
    {
       
        
        public OkCancelWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
