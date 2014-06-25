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
			tableLayoutPanel1.Controls.Add(navigatorControl);
		}
	}
}
