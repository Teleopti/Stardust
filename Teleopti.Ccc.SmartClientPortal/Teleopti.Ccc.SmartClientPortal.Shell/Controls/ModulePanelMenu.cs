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
	public partial class ModulePanelMenu : UserControl
	{
		public ModulePanelMenu()
		{
			InitializeComponent();
		}

		public event EventHandler ModulePanelMenuClick;

		protected virtual void OnModulePanelMenuClick(EventArgs e)
		{
			EventHandler handler = ModulePanelMenuClick;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void onMouseEnter(object sender, EventArgs e)
		{
			autoLabel1.ForeColor = Color.FromArgb(0, 153, 255);
		}

		private void onMouseLeave(object sender, EventArgs e)
		{
			autoLabel1.ForeColor = Color.Black;
		}

		private void onClick(object sender, EventArgs e)
		{
			OnModulePanelMenuClick(new EventArgs());
		}
	}
}
