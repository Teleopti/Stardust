using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday
{
    /// <summary>
    /// Interaction logic for StaffingEffectView.xaml
    /// </summary>
    public partial class StaffingEffectView : UserControl
    {
        public void SetDayLayerViewAdapterCollection(IDayLayerViewModel dayLayerViewModel)
        {
			Grid.DataContext = new StaffingEffectViewAdapter(dayLayerViewModel);
        }

        public StaffingEffectView()
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