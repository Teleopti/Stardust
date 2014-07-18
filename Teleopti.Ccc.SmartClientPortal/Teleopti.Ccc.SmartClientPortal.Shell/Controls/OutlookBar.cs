using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class OutlookBar : UserControl
	{
		private readonly IList<ModulePanelItem> _items = new List<ModulePanelItem>();
		private readonly ModulePanelMenu _menuLauncher = new ModulePanelMenu();

		public OutlookBar()
		{
			InitializeComponent();
			_menuLauncher.ModulePanelMenuClick += menuLauncherModulePanelMenuClick;	
		}

		public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

		public IEnumerable<ModulePanelItem> Items
		{
			get { return _items; }
		}

		public void ClearItems()
		{
			foreach (var modulePanelItem in Items)
			{
				modulePanelItem.ModulePanelItemClick -= modulePanelItemClick;
			}
			_items.Clear();
			flowLayoutPanel1.Controls.Clear();
		}

		public void AddItems(ModulePanelItem[] modulePanelItems)
		{
			foreach (var modulePanelItem in modulePanelItems)
			{
				modulePanelItem.Visible = true;
				_items.Add(modulePanelItem);
				modulePanelItem.ModulePanelItemClick += modulePanelItemClick;
				flowLayoutPanel1.Controls.Add(modulePanelItem);

				var menuItem = new ToolStripMenuItem(modulePanelItem.ItemText, modulePanelItem.ItemImage, modulePanelItemClick);
				menuItem.Tag = modulePanelItem;
				contextMenuStripEx1.Items.Add(menuItem);
			}
			flowLayoutPanel1.Controls.Add(_menuLauncher);
			_menuLauncher.Visible = true;

			hideOverflowedItems();
		}

		public void SelectItem(ModulePanelItem modulePanelItem)
		{
			if (!_items.Contains(modulePanelItem))
				return;

			modulePanelItemClick(modulePanelItem, new EventArgs());
		}

		protected virtual void OnSelectedItemChanged(SelectedItemChangedEventArgs e)
		{
			var handler = SelectedItemChanged;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			hideOverflowedItems();
		}

		void modulePanelItemClick(object sender, EventArgs e)
		{
			var item = sender as ModulePanelItem;
			if (item == null)
			{
				var menuItem = sender as ToolStripMenuItem;
				if (menuItem != null)
					item = menuItem.Tag as ModulePanelItem;
			}

			if(item == null)
				return;

			foreach (var modulePanelItem in Items)
			{
				modulePanelItem.SetSelected(false);
			}
			item.SetSelected(true);

			OnSelectedItemChanged(new SelectedItemChangedEventArgs(item));
		}

		void menuLauncherModulePanelMenuClick(object sender, EventArgs e)
		{
			_menuLauncher.ContextMenuStrip = contextMenuStripEx1;
			contextMenuStripEx1.Items[0].Text = Resources.CompactView;
			contextMenuStripEx1.Show(_menuLauncher, new Point(_menuLauncher.Width/2, _menuLauncher.Height/2));
			_menuLauncher.ContextMenuStrip = null;
		}

		private void navigationOptionsToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			foreach (var modulePanelItem in _items)
			{
				modulePanelItem.SetCompact(((ToolStripMenuItem)sender).Checked);
			}
			hideOverflowedItems();
		}

		private void hideOverflowedItems()
		{
			var allItemSize = 0;
			var hideAllFromThis = false;
			foreach (var modulePanelItem in _items)
			{
				allItemSize += modulePanelItem.Width + 6;
				if (allItemSize > flowLayoutPanel1.Width - (_menuLauncher.Width + 6))
					hideAllFromThis = true;

				modulePanelItem.ItemHidden = hideAllFromThis;
			}

			handleHiddenItems();
		}

		private void handleHiddenItems()
		{
			foreach (var modulePanelItem in _items)
			{
				foreach (var control in flowLayoutPanel1.Controls)
				{
					var controlPanel = control as ModulePanelItem;
					if (controlPanel != null && controlPanel.Equals(modulePanelItem))
						((Control)control).Visible = !modulePanelItem.ItemHidden;
				}
			}

			foreach (var item in contextMenuStripEx1.Items)
			{
				var toolStripMenuItem = item as ToolStripMenuItem;
				if (toolStripMenuItem != null && toolStripMenuItem.Tag is ModulePanelItem)
				{
					var modulePanelItem = (ModulePanelItem) toolStripMenuItem.Tag;
					toolStripMenuItem.Visible = modulePanelItem.ItemHidden;
				}
			}
		}
	}

	public class SelectedItemChangedEventArgs : EventArgs
	{
		private readonly ModulePanelItem _selectedItem;

		public SelectedItemChangedEventArgs(ModulePanelItem selectedItem)
		{
			_selectedItem = selectedItem;
		}

		public ModulePanelItem SelectedItem
		{
			get { return _selectedItem; }
		}
	}
}
