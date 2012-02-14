using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Form for letting the personItem decide actions when business rules are violating
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-19
    /// </remarks>
    public partial class HandleBusinessRuleResponse : BaseRibbonForm, IHandleBusinessRuleResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandleBusinessRuleResponse"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-19
        /// </remarks>
        public HandleBusinessRuleResponse()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            buttonOK.Enabled = false;
            SetColor();
        }
        private void SetColor()
        {
            BackColor = ColorHelper.DialogBackColor();
        }

        /// <summary>
        /// Sets the response from business rule validation.
        /// </summary>
        /// <param name="businessRulesResponse">The business rules respone.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-19
        /// </remarks>
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

                    ListViewItem listViewItem = new ListViewItem(response.Person.Name.ToString());
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



        /// <summary>
        /// Gets a value indicating whether [apply to all].
        /// </summary>
        /// <value><c>true</c> if [apply to all]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-24
        /// </remarks>
        public bool ApplyToAll
        {
            get { return checkBoxApplyToAll.Checked; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            IBusinessRuleResponse ruleResponse = listView1.Items[e.Index].Tag as IBusinessRuleResponse;
            if (ruleResponse != null &&
                ruleResponse.Mandatory)
                e.NewValue = CheckState.Unchecked;     
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
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
