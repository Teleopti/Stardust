using System.Windows;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Security;

namespace Teleopti.Ccc.WpfControls.Controls.PasswordPolicies
{
    /// <summary>
    /// Interaction logic for PasswordPolicyView.xaml
    /// </summary>
    public partial class PasswordPolicyServicePreview
    {
        public PasswordPolicyServicePreview()
        {
            InitializeComponent();
        }

      

        private void PickFile(object sender, RoutedEventArgs e)
        {
            var model = DataContext as PasswordPolicyServiceViewModel;
            if (model == null) return;
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Files|*.xml|UML Files|*.uml";
                openFileDialog.ShowDialog();
                model.Path = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            }
        }
    }
}
