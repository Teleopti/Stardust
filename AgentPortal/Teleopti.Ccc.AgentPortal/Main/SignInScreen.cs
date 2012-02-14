using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortal.SdkServiceReference;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Messaging.Core;

namespace Teleopti.Ccc.AgentPortal.Main
{
    public partial class SignInScreen : RibbonForm
    {
        #region Member Variables

        #endregion

        #region Constructor

        public SignInScreen()
        {
            InitializeComponent();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Users the authentication.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-05-07
        /// </remarks>
        private static bool UserAuthentication(string userName, string password)
        {
            if (SdkServiceHelper.InitializeSdkServiceHelper(userName, password) == 0)
            {
                PersonDto loggedPerson = SdkServiceHelper.AgentServiceClient.GetLoggedOnPerson();

                if (loggedPerson != null)
                {
                    // Intialize State Holder
                    StateHolder.Initialize(new StateManager());
                    StateHolder.Instance.State.SetSessionData(new SessionData(loggedPerson));

                    // If persons culture is then use machine culture
                    if (loggedPerson.CultureLanguageId == null)
                        loggedPerson.CultureLanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                    else
                        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo((int)loggedPerson.CultureLanguageId);

                    // If persons language is then use machine culture
                    if (loggedPerson.UICultureLanguageId == null)
                        loggedPerson.UICultureLanguageId = Thread.CurrentThread.CurrentUICulture.LCID;
                    else
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo((int)loggedPerson.UICultureLanguageId);

                    return true;
                }
            }
           
            return false;
        }

        /// <summary>
        /// Gets the custom error message.
        /// </summary>
        /// <param name="exceptionStatus">The exception status.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-02
        /// </remarks>
        private static string GetCustomErrorMessage(WebExceptionStatus exceptionStatus, string exceptionMessage)
        {
            string errorMessage = "";

            switch (exceptionStatus)
            {
                case WebExceptionStatus.ProtocolError:
                    errorMessage = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.InvalidCredential);
                    break;
                case WebExceptionStatus.ConnectFailure:
                    errorMessage = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.UnableToConnectRemoteService);
                    break;
            }

            return errorMessage;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the buttonLogon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-03-19
        /// </remarks>
        private void buttonSignIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (UserAuthentication(textBoxApplicationID.Text.Trim(), textBoxPassword.Text.Trim()))
                {
                    PermissionService.Instance().ValidatePermission(DefinedRaptorApplicationFunctionPaths.OpenAgentPortal);
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(UserTexts.Resources.InvalidCredential, UserTexts.Resources.AgentPortal,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                    (RightToLeft == RightToLeft.Yes)
                                        ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                        : 0);

                    textBoxApplicationID.SelectAll();
                    textBoxApplicationID.Focus();
                    DialogResult = System.Windows.Forms.DialogResult.None;
                }

            }
            catch (PermissionException perExp)
            {
                MessageBox.Show(perExp.Message, UserTexts.Resources.AgentPortal, MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0);



                // HACK: Handle this in a better way. /kosalanp.
                textBoxApplicationID.SelectAll();
                textBoxApplicationID.Focus();

                DialogResult = DialogResult.None;
            }
            catch (WebException ex)
            {
                string errorMessage = GetCustomErrorMessage(ex.Status, ex.Message);
                MessageBox.Show(errorMessage, UserTexts.Resources.AgentPortal, MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0);



                // HACK: Handle this in a better way. /kosalanp.
                errorMessage = null;
                textBoxApplicationID.SelectAll();
                textBoxApplicationID.Focus();

                DialogResult = DialogResult.None;
            }
            catch (BrokerNotInstantiatedException mbExp)
            {
                MessageBox.Show(mbExp.Message, UserTexts.Resources.AgentPortal, MessageBoxButtons.OK,
                               MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                               (RightToLeft == RightToLeft.Yes)
                                   ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                   : 0);
                textBoxApplicationID.SelectAll();
                textBoxApplicationID.Focus();

                DialogResult = DialogResult.None;

            }
        }

        /// <summary>
        /// Handles the Click event of the buttonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-03-19
        /// </remarks>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the KeyDown event of the textPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-05-07
        /// </remarks>
        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) //Enter Key
                buttonSignIn_Click(null, null);
        }

        /// <summary>
        /// Handles the Enter event of the textPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-05-07
        /// </remarks>
        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            textBoxPassword.SelectAll();
        }

        /// <summary>
        /// Handles the Enter event of the textUserName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-05-07
        /// </remarks>
        private void textBoxApplicationID_Enter(object sender, EventArgs e)
        {
            textBoxApplicationID.SelectAll();
        }

        #endregion
    }
}