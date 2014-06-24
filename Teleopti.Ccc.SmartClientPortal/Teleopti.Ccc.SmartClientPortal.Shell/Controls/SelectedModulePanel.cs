﻿using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class SelectedModulePanel : UserControl
	{
		public SelectedModulePanel()
		{
			InitializeComponent();
		}

		public void Set(ModulePanelItem modulePanel)
		{
			autoLabelHeader.Text = modulePanel.ModuleText;
			paneImagelHeader.BackgroundImage = modulePanel.ModuleImage;
		}
	}
}
