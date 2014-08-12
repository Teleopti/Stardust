using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
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
			IList<string> uniqueStrings = new List<string>();
			foreach (IBusinessRuleResponse response in businessRulesResponse)
			{
				if (response.Error)
				{
					if (uniqueStrings.Contains(response.Person.Name + response.Message))
						continue;

					uniqueStrings.Add(response.Person.Name + response.Message);
					if (response.Message.Length > longestMessage)
					{
						longestMessage = response.Message.Length;
					}

					var listViewItem = new ListViewItem(response.Person.Name.ToString());
					listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem,
																			   response.Message));

					listViewItem.Tag = response;
					listView1.Items.Add(listViewItem);
				}
			}
			Width = (longestMessage * 5) + 130;
			listView1.Columns[0].Width = 100;
			listView1.Columns[1].Width = listView1.Width - 5;
			//listView1.Width = (longestMessage * 3) + 30;
			
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
