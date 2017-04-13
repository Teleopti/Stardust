using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
	public partial class MessageBoxWithListView : BaseDialogForm
	{
		private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
		public MessageBoxWithListView(string text, string caption, IEnumerable<IUnsavedDayInfo> detailList)
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();

			setMessage(text);
			setCaption(caption);
			setListView(detailList);
		}

		private void setListView(IEnumerable<IUnsavedDayInfo> detailList)
		{
			listViewDetails.Items.Clear();
			var id = 1;
			foreach (var detail in detailList)
			{
				var item = new ListViewItem(Convert.ToString(id++, CultureInfo.CurrentCulture));
				item.SubItems.Add(new ListViewItem.ListViewSubItem(item, detail.DateTime.ToShortDateString(CultureInfo.CurrentUICulture)));
				item.SubItems.Add(new ListViewItem.ListViewSubItem(item, detail.Scenario.Description.Name));
				listViewDetails.Items.Add(item);
			}
			listViewDetails.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void setCaption(string caption)
		{
			Text = caption;
		}

		private void setMessage(string text)
		{
			lableMessage.Text = text;
		}

		private void listViewDetailsKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.C)
				copy();
			else if (e.Control && e.KeyCode == Keys.A)
				selectAllFromListView();
		}

		private void selectAllFromListView()
		{
			foreach (ListViewItem item in listViewDetails.Items)
			{
				item.Selected = true;
			}
		}

		private void copy()
		{
			_externalExceptionHandler.AttemptToUseExternalResource(Clipboard.Clear);
			var str = new StringBuilder();
			foreach (ColumnHeader header in listViewDetails.Columns)
			{
				str.Append(header.Text); str.Append('\t');
			}
			str.AppendLine();

			foreach (ListViewItem item in listViewDetails.SelectedItems)
			{
				foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
				{
					str.Append(subitem.Text); str.Append('\t');
				}
				str.AppendLine();
			}

			var dataObject = new DataObject();
			dataObject.SetData(DataFormats.Text, true, str);
			_externalExceptionHandler.AttemptToUseExternalResource(() => Clipboard.SetDataObject(dataObject, true));
		}

		private void listViewDetailsMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (listViewDetails.SelectedItems != null && listViewDetails.SelectedItems.Count > 0)
					ToolStripMenuItemCopy.Enabled = true;
			}
		}

		private void toolStripMenuItemCopyClick(object sender, EventArgs e)
		{
			copy();
		}

		private void buttonOkClick1(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
