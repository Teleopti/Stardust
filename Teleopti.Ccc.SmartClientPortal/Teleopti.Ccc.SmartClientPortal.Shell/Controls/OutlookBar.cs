using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class OutlookBar : UserControl
	{
		private readonly IList<ModulePanelItem> _items = new List<ModulePanelItem>();

		public OutlookBar()
		{
			InitializeComponent();
			//if (DesignMode)
			//	AddItem(new ModulePanelItem());
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

		public void AddItem(ModulePanelItem modulePanelItem)
		{
			modulePanelItem.Visible = true;
			_items.Add(modulePanelItem);
			modulePanelItem.ModulePanelItemClick += modulePanelItemClick;
			flowLayoutPanel1.Controls.Add(modulePanelItem);
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

		void modulePanelItemClick(object sender, EventArgs e)
		{
			var item = ((ModulePanelItem) sender);
			foreach (var modulePanelItem in Items)
			{
				modulePanelItem.SetSelected(false);
			}
			item.SetSelected(true);

			OnSelectedItemChanged(new SelectedItemChangedEventArgs(item));
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
