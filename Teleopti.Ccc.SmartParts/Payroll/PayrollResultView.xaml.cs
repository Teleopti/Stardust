﻿using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartParts.Payroll
{
    public partial class PayrollResultView : UserControl
    {
        public PayrollResultView()
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
