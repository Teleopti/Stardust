using System;
using System.ServiceModel;
using System.Web.Services.Protocols;
using Teleopti.Ccc.AgentPortal.Common;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Settings
{
    public partial class ChangePasswordControl : BaseUserControl, IDialogControl
    {
        private bool _controlWasActive;

        public void InitializeDialogControl()
        {
            autoLabelUserName.Text = autoLabelUserName.Text + " " + StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.Name;
        }

        public void LoadControl()
        {
        }

        public string TreeFamily()
        {
            return UserTexts.Resources.AgentSettings;
        }

        public string TreeNode()
        {
            return UserTexts.Resources.PasswordSettings;
        }

        public bool SaveChanges()
        {
            if (!_controlWasActive) return true;

            if(!ValidateData())
            {
                string validationErrorMessage = UserTexts.Resources.PasswordsDoNotMatch;
                throw new PasswordMismatchException(validationErrorMessage);
            }
            
            PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            string oldPassword = textBoxExtOldPassword.Text;
            string newPassword = textBoxExtNewPassword.Text;

            if (newPassword != oldPassword)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    var changed = SdkServiceHelper.LogOnServiceClient.ChangePassword(person, oldPassword, newPassword);
                    if (!changed)
                    {
                        string validationErrorMessage = UserTexts.Resources.PasswordsDoNotMatch;
                        throw new PasswordMismatchException(validationErrorMessage);
                    }
                    StateHolder.Instance.StateReader.SessionScopeData.SetPassword(newPassword);
                }

                catch (FaultException ex)
                {
                    MessageBoxHelper.ShowErrorMessage(ex.Message, UserTexts.Resources.AgentPortal);
                    return false;
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            return true;
        }

        public ChangePasswordControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        private bool ValidateData()
        {
            string pswd = textBoxExtNewPassword.Text;
            string confirm = textBoxExtConfirmPassword.Text;

            if (String.IsNullOrEmpty(pswd) || String.IsNullOrEmpty(confirm) || !pswd.Equals(confirm))
                return false;

            return true;
        }

        private void textBoxExt_Leave(object sender, EventArgs e)
        {
            _controlWasActive = true;
        }
    }
}
