using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class ModulePanelItem : UserControl
	{
		private bool _selected;
		private bool _itemEnabled = true;
		private bool _itemHidden;
		private string _itemText;
		private bool _compact;

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
			get { return _itemText; }
			set
			{
				_itemText = value;
				toolTip1.SetToolTip(panelImage, _itemText);
				if (!_compact)
					labelModuleText.Text = _itemText;
			}
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

		public bool ItemHidden
		{
			get { return _itemHidden; }
			set { _itemHidden = value; }
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
			setDefaultColors();
		}

		private void setDefaultColors()
		{
			if (!_itemEnabled)
				return;

			labelModuleText.ForeColor = _selected ? Color.FromArgb(0, 153, 255) : Color.Black;
			panelImage.BackColor = _compact && _selected ? Color.SkyBlue : Color.FromKnownColor(KnownColor.Control);
		}

		public void SetCompact(bool compact)
		{
			_compact = compact;
			labelModuleText.Text = _compact ? string.Empty : _itemText;
			toolTip1.Active = _compact;
			setDefaultColors();
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
			if (_compact)
				panelImage.BackColor = Color.SkyBlue;
		}

		private void onMouseLeave(object sender, EventArgs e)
		{
			if (_selected || !_itemEnabled)
				return;

			labelModuleText.ForeColor = Color.Black;
			if (_compact)
				panelImage.BackColor = Color.FromKnownColor(KnownColor.Control);
		}

		private void onClick(object sender, EventArgs e)
		{
			OnModulePanelClick(new EventArgs());
		}
	}
}
