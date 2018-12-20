using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class ValidationAlertsView : BaseUserControl
	{
		private ValidationAlertsModel _model;
		private int _sortColumn = -1;

		private IEnumerable<ValidationAlertsModel.ValidationAlert> _latestValidationAlertList =
			new List<ValidationAlertsModel.ValidationAlert>();

		public ValidationAlertsView()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

		public event EventHandler<ValidationViewAgentDoubleClickEvenArgs> AgentDoubleClick;

		public void SetModel(ValidationAlertsModel model)
		{
			_model = model;
		}

		public void ReDraw(IEnumerable<IPerson> filteredPersons)
		{			
			_latestValidationAlertList = _model.GetAlerts(filteredPersons);
			var uniqueAlertTypes = new HashSet<string>();
			foreach (var validationAlert in _latestValidationAlertList)
			{
				uniqueAlertTypes.Add(validationAlert.TypeName);
			}

			addToolStripMenuItems(uniqueAlertTypes);
			removeToolStripMenuItems(uniqueAlertTypes);
			fillFiltereListView();
			if (_sortColumn == -1)
			{
				doSort(0);
			}
		}

		private void addToolStripMenuItems(IEnumerable<string> uniqueAlertTypes)
		{
			foreach (var alertType in uniqueAlertTypes)
			{
				var found = false;
				foreach (var dropDownItem in toolStripDropDownButtonFilter.DropDownItems)
				{
					var item = dropDownItem as ToolStripMenuItem;
					if (item == null)
						continue;

					if ((string)item.Tag == alertType)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					var toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Checked = true;
					toolStripMenuItem.CheckOnClick = true;
					toolStripMenuItem.Text = alertType;
					toolStripMenuItem.Tag = alertType;
					toolStripMenuItem.CheckedChanged += toolStripMenuItemCheckedChanged;
					toolStripDropDownButtonFilter.DropDownItems.Add(toolStripMenuItem);
				}
			}
		}

		private void removeToolStripMenuItems(IEnumerable<string> uniqueAlertTypes)
		{
			for (int i = 1; i < toolStripDropDownButtonFilter.DropDownItems.Count; i++)
			{
				var item = toolStripDropDownButtonFilter.DropDownItems[i] as ToolStripMenuItem;
				if (item == null)
					continue;

				var found = false;
				foreach (var uniqueAlertType in uniqueAlertTypes)
				{
					if ((string)item.Tag == uniqueAlertType)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					item.CheckedChanged -= toolStripMenuItemCheckedChanged;
					toolStripDropDownButtonFilter.DropDownItems.RemoveAt(i);
					i--;
				}
			}
		}

		private void fillFiltereListView()
		{
			listView1.BeginUpdate();
			listView1.Items.Clear();
			foreach (var validationAlert in _latestValidationAlertList)
			{
				foreach (var dropDownItem in toolStripDropDownButtonFilter.DropDownItems)
				{
					var item = dropDownItem as ToolStripMenuItem;
					if (item == null)
						continue;

					if ((string)item.Tag == validationAlert.TypeName && item.Checked)
					{
						var listItem = new ListViewItem {Text = validationAlert.Date.ToShortDateString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture)};
						listItem.SubItems.Add(validationAlert.PersonName);
						listItem.SubItems.Add(validationAlert.Alert);
						listItem.Tag = new Tuple<DateOnly, IPerson>(validationAlert.Date, validationAlert.Person);
						listItem.ToolTipText = validationAlert.Alert;
						listView1.Items.Add(listItem);
						break;
					}
				}
			}
			listView1.EndUpdate();
		}

		private void toolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			fillFiltereListView();
		}

		private void listView1MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listView1.SelectedItems.Count == 0)
				return;

			var dateOnlyPersonTuple = listView1.SelectedItems[0].Tag as Tuple<DateOnly, IPerson>;
			if (dateOnlyPersonTuple == null)
				return;

			OnAgentDoubleClick(new ValidationViewAgentDoubleClickEvenArgs(dateOnlyPersonTuple.Item1, dateOnlyPersonTuple.Item2));
		}

		protected virtual void OnAgentDoubleClick(ValidationViewAgentDoubleClickEvenArgs e)
		{
			AgentDoubleClick?.Invoke(this, e);
		}

		private void toolStripButtonFindClick(object sender, EventArgs e)
		{
			listView1MouseDoubleClick(sender, new MouseEventArgs(MouseButtons.Left, 2,1,1,0));
		}

		private void allToolStripMenuItemClick(object sender, EventArgs e)
		{
			for (int i = 1; i < toolStripDropDownButtonFilter.DropDownItems.Count; i++)
			{
				var item = toolStripDropDownButtonFilter.DropDownItems[i] as ToolStripMenuItem;
				if (item == null)
					continue;

				item.Checked = true;
			}
		}

		private void listView1ColumnClick(object sender, ColumnClickEventArgs e)
		{
			var column = e.Column;
			doSort(column);
		}

		private void doSort(int column)
		{
			if (column != _sortColumn)
			{
				_sortColumn = column;
				listView1.Sorting = SortOrder.Ascending;
			}
			else
			{
				if (listView1.Sorting == SortOrder.Ascending)
					listView1.Sorting = SortOrder.Descending;
				else
					listView1.Sorting = SortOrder.Ascending;
			}

			listView1.Sort();
			listView1.ListViewItemSorter = new ListViewItemComparer(column,
				listView1.Sorting);
		}
	}

	public class ValidationViewAgentDoubleClickEvenArgs : EventArgs
	{
		public ValidationViewAgentDoubleClickEvenArgs(DateOnly date, IPerson person)
		{
			Date = date;
			Person = person;
		}

		public DateOnly Date { get; }
		public IPerson Person { get; }
	}

	public class ListViewItemComparer : IComparer
	{
		private int _col;
		private readonly SortOrder _order;

		public ListViewItemComparer()
		{
			_col = 0;
			_order = SortOrder.Ascending;
		}
		public ListViewItemComparer(int column, SortOrder order)
		{
			_col = column;
			_order = order;
		}

		public int Compare(object x, object y)
		{
			var ix = x as ListViewItem;
			var iy = y as ListViewItem;
			if (iy == null || ix == null)
				return 0;

			if (ix.SubItems.Count <= _col || iy.SubItems.Count <= _col)
				return 0;

			int returnVal = string.CompareOrdinal(ix.SubItems[_col].Text, iy.SubItems[_col].Text);

			if (_order == SortOrder.Descending)
				returnVal *= -1;

			return returnVal;
		}
	}
}
