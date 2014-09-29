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
		private Control _navigatorControl;

		public OutlookBarWorkSpace()
		{
			InitializeComponent();
		}

		public Control NavigatorControl
		{
			get { return _navigatorControl; }
		}

		public void SetHeader(ModulePanelItem selectedModulePanelItem)
		{
			selectedModulePanel1.Set(selectedModulePanelItem);
		}

		public void SetNavigatorControl(Control navigatorControl)
		{
			panel1.Controls.Clear();
			panel1.Controls.Add(navigatorControl);
			_navigatorControl = navigatorControl;
			navigatorControl.Dock = DockStyle.Fill;
			navigatorControl.Focus();
		}
	}
}
