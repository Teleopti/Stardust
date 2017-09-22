using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	public partial class HandleBusinessRuleResponse : BaseDialogForm, IHandleBusinessRuleResponse
	{
		public HandleBusinessRuleResponse()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			buttonOK.Enabled = false;
		}
		
		public void SetResponse(IEnumerable<IBusinessRuleResponse> businessRulesResponse)
		{
			int longestMessage = 0;
			checkBoxApplyToAll.Checked = false;
			listView1.Items.Clear();

			var items = businessRulesResponse.Where(r => r.Error).GroupBy(k => new { Name=k.Person.Name.ToString(), k.Message }).Select(
				r =>
				{
					var listViewItem = new ListViewItem(r.Key.Name);
					listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem,
																			   r.Key.Message));
					if (r.Key.Message.Length > longestMessage)
					{
						longestMessage = r.Key.Message.Length;
					}

					var firstResponse = r.First();
					listViewItem.Tag = firstResponse;
					return listViewItem;
				}).ToArray();
			listView1.Items.AddRange(items);

			Width = (longestMessage * 5) + 130;
			listView1.Columns[0].Width = 100;
			listView1.Columns[1].Width = listView1.Width - 5;
			
			buttonOK.Enabled = true;

			ShowDialog();
		}

		public bool ApplyToAll
		{
			get { return checkBoxApplyToAll.Checked; }
		}

		private void buttonOkClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void buttonCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void listView1ItemCheck(object sender, ItemCheckEventArgs e)
		{
			var ruleResponse = listView1.Items[e.Index].Tag as IBusinessRuleResponse;
			if (ruleResponse != null &&
				ruleResponse.Mandatory)
				e.NewValue = CheckState.Unchecked;     
		}

		private void listView1ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			bool enable = true;

			foreach (ListViewItem listViewItem in listView1.Items)
			{
				if (listViewItem.Checked == false)
				{
					enable = false;
					break;
				}
			}

			buttonOK.Enabled = enable;
		}
	}
}
