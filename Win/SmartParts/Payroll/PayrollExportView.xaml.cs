using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.SmartParts.Payroll
{
    /// <summary>
    /// Interaction logic for PayrollExportView.xaml
    /// </summary>
    public partial class PayrollExportView : UserControl
    {
        public PayrollExportView()
        {
            InitializeComponent();
        }

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			string helpContext = HelpProvider.GetHelpString(this);
			if (string.IsNullOrEmpty(helpContext) && Parent == null) return;
			var elementParent = Parent as FrameworkElement;
			if (elementParent == null || elementParent.Parent == null) return;
			HelpProvider.SetHelpString(elementParent.Parent, helpContext);
		}
    }
}
