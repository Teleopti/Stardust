using System.Windows;
using System.Windows.Input;
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

		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				OkButton.Focus();
				if (OkButton.IsEnabled)
					DialogResult = true;
			}
			base.OnPreviewKeyUp(e);
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
