using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class OutlookBarWorkSpace : UserControl
	{
		public OutlookBarWorkSpace()
		{
			InitializeComponent();
		}

		public void SetHeader(ModulePanelItem selectedModulePanelItem)
		{
			selectedModulePanel1.Set(selectedModulePanelItem);
		}

		public void SetNavigatorControl(Control navigatorControl)
		{
			panel1.Controls.Clear();
			panel1.Controls.Add(navigatorControl);
			navigatorControl.Dock = DockStyle.Fill;
			navigatorControl.Focus();
		}
	}
}
