using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class RequestReplyDialog : BaseRibbonForm
    {
        private IList<PersonRequestViewModel> _requestViewAdapterlist;

        public RequestReplyDialog(IList<PersonRequestViewModel> list)
        {
            _requestViewAdapterlist = list;

            InitializeComponent();

            SetColors();
            if (!DesignMode) SetTexts();

            // TODO: check lengths available in notes field and set textbox maxlength accordingly

            if (list.Count == 1)
            {
                textBoxMessage.Visible = true;
                textBoxMessage.Text = list[0].GetMessage(new NoFormatting());
            }
            else if (list.Count > 1)
            {
                textBoxMessage.Visible = false;
                textBoxReply.Dock = DockStyle.Fill;
            }
        }

        private void SetColors()
        {
            BackColor = ColorHelper.StandardPanelBackground();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonReply_Click(object sender, EventArgs e)
        {
            IList<PersonRequestViewModel> errorList = new List<PersonRequestViewModel>();
            foreach(PersonRequestViewModel adapter in _requestViewAdapterlist)
            {
                if (adapter.IsPending)
                {
                        if (!adapter.PersonRequest.Reply(textBoxReply.Text))
                            errorList.Add(adapter);
                }
            }
            if (errorList.Count >0)
            {
                string message = UserTexts.Resources.OneOrMoreMessagesWereTooLongPleaseTryAShorterMessage;
                ShowErrorMessage(message, UserTexts.Resources.MessageTooLong);
                _requestViewAdapterlist = errorList;
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
