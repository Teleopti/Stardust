using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class ModulePanelItem : UserControl
	{
		private bool _selected;
		private bool _itemEnabled = true;

		public ModulePanelItem()
		{
			InitializeComponent();
		}

		public event EventHandler ModulePanelItemClick;

		public Image ItemImage
		{
			get { return panelImage.BackgroundImage; }
			set { panelImage.BackgroundImage = value; }
		}

		public string ItemText
		{
			get { return labelModuleText.Text; }
			set { labelModuleText.Text = value; }
		}

		public bool ItemEnabled
		{
			get { return _itemEnabled; }
			set
			{
				_itemEnabled = value;
				labelModuleText.ForeColor = _itemEnabled ? Color.Black : Color.DarkGray;
			}
		}

		public bool Selected()
		{
			return _selected;
		}

		public void SetSelected(bool selected)
		{
			if (!_itemEnabled)
				return;

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
			if (_selected || !_itemEnabled)
				return;
			
			labelModuleText.ForeColor = Color.FromArgb(0, 153, 255);
		}

		private void onMouseLeave(object sender, EventArgs e)
		{
			if (_selected || !_itemEnabled)
				return;

			labelModuleText.ForeColor = Color.Black;
		}

		private void onClick(object sender, EventArgs e)
		{
			OnModulePanelClick(new EventArgs());
		}
	}
}
