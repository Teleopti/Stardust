using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class ModulePanelItem : UserControl
	{
		private bool _selected;

		public ModulePanelItem()
		{
			InitializeComponent();
		}

		public event EventHandler ModulePanelItemClick;

		public Image ModuleImage
		{
			get { return panelImage.BackgroundImage; }
			set { panelImage.BackgroundImage = value; }
		}

		public string ModuleText
		{
			get { return labelModuleText.Text; }
			set { labelModuleText.Text = value; }
		}

		public bool Selected()
		{
			return _selected;
		}

		public void SetSelected(bool selected)
		{
			_selected = selected;
			labelModuleText.ForeColor = selected ? Color.FromArgb(0, 153, 255) : Color.Black;
		}

		protected virtual void OnModulePanelClick(EventArgs e)
		{
			EventHandler handler = ModulePanelItemClick;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void onMouseEnter(object sender, EventArgs e)
		{
			if (_selected)
				return;
			
			labelModuleText.ForeColor = Color.FromArgb(0, 153, 255);
		}

		private void onMouseLeave(object sender, EventArgs e)
		{
			labelModuleText.ForeColor = Color.Black;
		}

		private void onClick(object sender, EventArgs e)
		{
			OnModulePanelClick(new EventArgs());
		}
	}
}
