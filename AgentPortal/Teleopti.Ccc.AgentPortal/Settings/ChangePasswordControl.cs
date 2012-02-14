using System;
using System.Web.Services.Protocols;
using Teleopti.Ccc.AgentPortal.Common;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Settings
{
    /// <summary>
    /// Represent the user control that allow user to change his/her password settings
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-08-18
    /// </remarks>
    public partial class ChangePasswordControl : BaseUserControl, IDialogControl
    {
        private bool _controlWasActive;

        /// <summary>
        /// Manually initialze control components. Calls when OptionDialog contructor.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void InitializeDialogControl()
        {
            autoLabelUserName.Text = autoLabelUserName.Text + " " + StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.Name;
        }

        /// <summary>
        /// Manually load control details. Calls when OptionDialog loads.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void LoadControl()
        {
           
        }

        /// <summary>
        /// The name of the Parent if represented in a TreeView
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public string TreeFamily()
        {
            return UserTexts.Resources.AgentSettings;
        }

        /// <summary>
        /// The name of the Node if represented in a TreeView
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public string TreeNode()
        {
            return UserTexts.Resources.PasswordSettings;
        }

        /// <summary>
        /// Persist all and save changes by the control. Calls when OkDialog hits.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
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
                    bool changed;
                    bool resultSpecified;
                    SdkServiceHelper.LogOnServiceClient.ChangePassword(person, oldPassword, newPassword, out changed,
                                                                       out resultSpecified);
                    if (!changed)
                    {
                        string validationErrorMessage = UserTexts.Resources.PasswordsDoNotMatch;
                        throw new PasswordMismatchException(validationErrorMessage);
                    }
                    StateHolder.Instance.StateReader.SessionScopeData.SetPassword(newPassword);
                }

                catch (SoapException ex)
                {
                    MessageBox.Show(ex.Message, UserTexts.Resources.AgentPortal, MessageBoxButtons.OK,
                                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                    (RightToLeft == RightToLeft.Yes)
                                        ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                        : 0);
                    return false;
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            return true;
        }

        /// <summary>
        /// Persist all and delete data by the control. Calls when DeleteDialog hits.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void Delete()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordControl"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public ChangePasswordControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Validates the data.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/3/2008
        /// </remarks>
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
