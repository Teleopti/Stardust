using System;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class RequestReplyStatusChangeDialog : BaseRibbonForm
    {
        private IList<PersonRequestViewModel> _requestViewAdapterlist;
        private readonly IHandlePersonRequestCommand _command;
        private readonly IRequestPresenter _requestPresenter;
        private bool _setStatus;

        public RequestReplyStatusChangeDialog(IRequestPresenter requestPresenter, IList<PersonRequestViewModel> list, IHandlePersonRequestCommand command)
        {
            _requestPresenter = requestPresenter;
            _requestViewAdapterlist = list;
            _command = command;
            InitializeComponent();

            SetColors();
            if (!DesignMode) SetTexts();

            gradientPanelButtons.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            gradientPanelMain.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            // TODO: check lengths available in notes field and set textbox maxlength accordingly

            if (list.Count == 1)
            {
                textBoxMessage.Visible = true;
                textBoxMessage.Clear();
                //?Primitivt, 2009, eller?
                textBoxMessage.Lines = list[0].Message.Split(Environment.NewLine.ToCharArray());
                //textBoxMessage.Text = list[0].Note;
            }
            else if (list.Count > 1)
            {
                textBoxMessage.Visible = false;
                textBoxReply.Dock = DockStyle.Fill;
            }

            _setStatus = true;
        }

        public RequestReplyStatusChangeDialog(IRequestPresenter requestPresenter, IList<PersonRequestViewModel> list)
        {
            _requestViewAdapterlist = list;
            _requestPresenter = requestPresenter;
            InitializeComponent();

            SetColors();
            if (!DesignMode) SetTexts();

            gradientPanelButtons.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            gradientPanelMain.BackgroundColor = ColorHelper.ControlGradientPanelBrush();

            // TODO: check lengths available in notes field and set textbox maxlength accordingly

            if (list.Count == 1)
            {
                textBoxMessage.Visible = true;
                textBoxMessage.Clear();
                //?Primitivt, 2009, eller?
                textBoxMessage.Lines = list[0].Message.Split(Environment.NewLine.ToCharArray());
                //textBoxMessage.Text = list[0].Note;
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
            if (_setStatus)
                ReplySetStatus();
            else
                Reply();
        }

        private void ReplySetStatus()
        {
            IList<PersonRequestViewModel> changeList = new List<PersonRequestViewModel>();
            IList<PersonRequestViewModel> errorList = new List<PersonRequestViewModel>();

            foreach (PersonRequestViewModel adapter in _requestViewAdapterlist)
            {
                if (adapter.IsPending)
                {
                    if (adapter.PersonRequest.CheckReplyTextLength(textBoxReply.Text))
                        changeList.Add(adapter);
                    else
                        errorList.Add(adapter);
                }
            }

            if (changeList.Count > 0)
            {
                _requestPresenter.ApproveOrDeny(changeList, _command, textBoxReply.Text);
            }
            if (errorList.Count > 0)
            {
                showMessage();
                _requestViewAdapterlist = errorList;
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Reply()
        {
            IList<PersonRequestViewModel> changeList = new List<PersonRequestViewModel>();
            IList<PersonRequestViewModel> errorList = new List<PersonRequestViewModel>();
            foreach (PersonRequestViewModel adapter in _requestViewAdapterlist)
            {
                if (adapter.IsPending)
                {

                    if (adapter.PersonRequest.CheckReplyTextLength(textBoxReply.Text))
                        changeList.Add(adapter);
                    else
                        errorList.Add(adapter);
                }
            }
            if (changeList.Count > 0)
            {
                _requestPresenter.Reply(changeList, textBoxReply.Text);    
            }

            if (errorList.Count > 0)
            {
                showMessage();
                _requestViewAdapterlist = errorList;
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void showMessage()
        {
            string message = UserTexts.Resources.OneOrMoreMessagesWereTooLongPleaseTryAShorterMessage;
            MessageBox.Show(
                message,
                UserTexts.Resources.MessageTooLong,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                (RightToLeft == RightToLeft.Yes)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);
        }
    }
}
