using System;
using System.Collections.Generic;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Scheduling
{
    /// <summary>
    /// Interaction logic for BusinessRuleResponseWindow.xaml
    /// </summary>
    public partial class BusinessRuleResponseDialog : Window
    {
        public BusinessRuleResponseDialog()
        {
            InitializeComponent();
        }

        public BusinessRuleResponseDialog(BusinessRuleResponseListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowDialogFromWinForms(IEnumerable<IBusinessRuleResponse> businessRuleResponses)
        {
            var viewModel = new BusinessRuleResponseListViewModel(businessRuleResponses);
            var businessRuleResponseDialog = new BusinessRuleResponseDialog(viewModel);
            businessRuleResponseDialog.ShowDialogFromWinForms(true, TimeZoneInfo.Local);
        }
    }
}
