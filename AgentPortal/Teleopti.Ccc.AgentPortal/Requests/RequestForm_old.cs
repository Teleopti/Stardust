using System;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Requests
{
    [CLSCompliant(true)]
    public partial class RequestForm : BaseRibbonForm
    {
        private IRequestFormControl _requestFormControl;

        public RequestForm()
        {
            InitializeComponent();
            SetTexts();
        }

        public RequestForm(IRequestFormControl requestControl, MessageBoxButtons messageBoxButtons) : this()
        {
            switch (messageBoxButtons)
            {
                case MessageBoxButtons.OKCancel:
                    
                    break;
                case MessageBoxButtons.OK:
                    buttonAdvOK.Visible = false;
                    buttonAdvCancel.Text = UserTexts.Resources.Close;
                    break;
            }
            _requestFormControl = requestControl;
        }

        private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            SetDialogResult(DialogResult.OK);
            Close();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            SetDialogResult(DialogResult.Cancel);
            Close();
        }

        private void RequestForm_Load(object sender, EventArgs e)
        {
            toolStripExMain.Enabled = false;
            toolStripExDelete.Enabled = false;
            toolStripTabItemResponse.Enabled = true;
            ((Control) _requestFormControl).Dock = DockStyle.Fill;
            gradientPanelMain.Controls.Add((Control) _requestFormControl);

            ShiftTradeRequestDto shiftTradeRequestDto = null;

            if(_requestFormControl.PersonRequestDto!=null)
                shiftTradeRequestDto = _requestFormControl.PersonRequestDto.Request as ShiftTradeRequestDto;

            //Buttons should be disabled if allreadey approved or denied
            if (shiftTradeRequestDto != null)
            {
                PersonDto personDto = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson;
                bool samePerson = (personDto.Id == shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonTo.Id);
                bool shiftTradeStatus = (shiftTradeRequestDto.ShiftTradeStatus == ShiftTradeStatusDto.OkByMe);
                bool requestStatus = (_requestFormControl.PersonRequestDto.RequestStatus == RequestStatusDto.Pending);
                toolStripExMain.Enabled = shiftTradeStatus & requestStatus & samePerson;

                //If not yet accepted by the other party, tou can delete the shifttrade
                toolStripExDelete.Enabled = shiftTradeStatus & !samePerson;
            }
        }



        private void toolStripButtonAccept_Click(object sender, EventArgs e)
        {
            _requestFormControl.Accept();
            SetDialogResult(DialogResult.OK);
        }

        private void toolStripButtonDeny_Click(object sender, EventArgs e)
        {
            _requestFormControl.Deny();
            SetDialogResult(DialogResult.OK);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            _requestFormControl.Delete();
            SetDialogResult(DialogResult.OK);
        }
    }
}
