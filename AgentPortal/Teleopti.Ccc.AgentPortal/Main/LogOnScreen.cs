﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Main
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
        MessageId = "OnScreen")]
    public partial class LogOnScreen : Form
    {
        private readonly LogOnDetails _logOnDetails;
        private readonly IDictionary<string, string> _appSettings;

        private enum LoadingState
        {
            Initializing = 0,
            LoggingIn = 1,
            Loading = 2,
            Verifying = 3,
            Ready = 4
        }

        /// <summary>
        /// Occurs when the authentication is done.
        /// </summary>
        public event EventHandler<EventArgs> AuthenticationDone;

        private IList<DataSourceDto> _logonableWindowsDataSources;
        private IList<DataSourceDto> _availableApplicationDataSources;
        private IList<BusinessUnitDto> _businessUnitList;
        private bool _cancelLogOn;
        private LoadingState _loadingState = LoadingState.Initializing;
        private DataSourceDto _choosenDataSource;
        private BusinessUnitDto _choosenBusinessUnit;
        private bool _dataSourceLogonable;

        public AuthenticationTypeOptionDto AuthenticationType { get; set; }

        public LogOnScreen()
        {
            AuthenticationType = AuthenticationTypeOptionDto.Windows;
            InitializeComponent();
        }

        public LogOnScreen(LogOnDetails logOnDetails,IDictionary<string,string> appSettings) : this()
        {
            _logOnDetails = logOnDetails;
            _appSettings = appSettings;

            if (_logOnDetails.DataSource != null)
            {
                AuthenticationType = AuthenticationTypeOptionDto.Application;
                _choosenDataSource = _logOnDetails.DataSource;
            }
        }

        /// <summary>
        /// Handles the Load event of the Form control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Form_Load(object sender, EventArgs e)
        {
            SetTexts();
            ActivatePanel(panelPicture);
            ViewState();
            buttonLogOnOK.Enabled = false;
            textBoxLogOnName.TextChanged += (LogOnTextChanged);
            textBoxPassword.TextChanged += (LogOnTextChanged);
            Show();
            Refresh();

            var service = SdkServiceHelper.LogOnServiceClient;
            Authenticate(service);

            if (_cancelLogOn)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (!VerifyLicense())
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            OnAuthenticationDone();

            if (_cancelLogOn)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (!LoggedOnPersonDataLoaded())
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            SynchronizeRaptorFunctions();

            NextState();

            NextState();

            if (Authorize())
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            CreateSettings();

            NextState();

            DialogResult = DialogResult.OK;

            Close();
        }

        void LogOnTextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxLogOnName.Text) || string.IsNullOrEmpty(textBoxPassword.Text))
            {
                buttonLogOnOK.Enabled = false;
                return;
            }
            buttonLogOnOK.Enabled = true;
        }

        private void tabControlChooseDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlChooseDataSource.SelectedTab.Equals(tabPageWindowsDataSources))
            {
                AuthenticationType = AuthenticationTypeOptionDto.Windows;
                listBoxWindowsDataSources.Focus();
            }
            else
            {
                AuthenticationType = AuthenticationTypeOptionDto.Application;
                listBoxApplicationDataSources.Focus();
            }
        }

        private void buttonLogOnOK_Click(object sender, EventArgs e)
        {
            _logOnDetails.UserName = textBoxLogOnName.Text;
            _logOnDetails.Password = textBoxPassword.Text;

            signOn();
        }

        private void signOn()
        {
            if (_choosenDataSource == null) return;
            var currentHeader = Sdk.Client.AuthenticationSoapHeader.Current;
            currentHeader.DataSource = _choosenDataSource.Name;
            currentHeader.UseWindowsIdentity = false;

            var resultDto =
                SdkServiceHelper.LogOnServiceClient.LogOnApplicationUser(_logOnDetails.UserName, _logOnDetails.Password,
                                                                         _choosenDataSource);
            _businessUnitList = resultDto.BusinessUnitCollection;
            if (resultDto.HasMessage)
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, string.Concat(resultDto.Message, "  "),
                                                            UserTexts.Resources.LogOn,
                                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
                                                            MessageBoxDefaultButton.Button1,
                                                            (RightToLeft == RightToLeft.Yes)
                                                                ? MessageBoxOptions.RtlReading |
                                                                  MessageBoxOptions.RightAlign
                                                                : 0);
            }

            _dataSourceLogonable = resultDto.Successful;
        }

        private void buttonDataSourcesListOK_Click(object sender, EventArgs e)
        {
            if (AuthenticationType == AuthenticationTypeOptionDto.Windows)
            {
                _choosenDataSource = (DataSourceDto)listBoxWindowsDataSources.SelectedItem;
            }
            else
            {
                _choosenDataSource = (DataSourceDto)listBoxApplicationDataSources.SelectedItem;
            }
        }

        private void buttonBusinessUnitsOK_Click(object sender, EventArgs e)
        {
            _choosenBusinessUnit = (BusinessUnitDto) listBoxBusinessUnits.SelectedItem;
        }

        private void buttonLogOnCancel_Click(object sender, EventArgs e)
        {
            _cancelLogOn = true;
        }

        /// <summary>
        /// Called when raising the AuthenticationDone event.
        /// </summary>
        protected virtual void OnAuthenticationDone()
        {
            if (AuthenticationDone != null)
                AuthenticationDone(this, EventArgs.Empty);
        }

        private void ActivatePanel(Panel panel)
        {
            SuspendLayout();
            panelChooseBusinessUnit.Visible = false;
            panelChooseDataSource.Visible = false;
            panelLogin.Visible = false;
            panelPicture.Visible = false;

            panel.Dock = DockStyle.Fill;
            panel.Visible = true;

            ResumeLayout(true);
            Refresh();
            Application.DoEvents();
        }

        /// <summary>
        /// Moves to the next loading state.
        /// </summary>
        private void NextState()
        {
            _loadingState++;
            ViewState();
        }

        private void ViewState()
        {
            ShowInTaskbar = false;
#if (DEBUG)
            TopMost = false;
#else
            TopMost = true;
#endif
            switch (_loadingState)
            {
                case LoadingState.Initializing:
                    labelStatusText.Text = UserTexts.Resources.InitializingTreeDots;
                    break;
                case LoadingState.LoggingIn:
                    ShowInTaskbar = true;
                    SuspendLayout();
                    pictureBoxStep2.Dock = DockStyle.Fill;
                    pictureBoxStep2.Visible = true;
                    pictureBoxStep1.Visible = false;
                    ResumeLayout();
                    labelStatusText.Text = UserTexts.Resources.LoggingOnTreeDots;
                    TopMost = false;
                    break;
                case LoadingState.Loading:
                    labelStatusText.Text = UserTexts.Resources.LoadingDataTreeDots;
                    break;
                case LoadingState.Verifying:
                    SuspendLayout();
                    pictureBoxStep3.Dock = DockStyle.Fill;
                    pictureBoxStep3.Visible = true;
                    pictureBoxStep2.Visible = false;
                    ResumeLayout();
                    labelStatusText.Text = UserTexts.Resources.VerifyingPermissionsTreeDots;
                    break;
                case LoadingState.Ready:
                    break;
            }

            Refresh();
        }

        private void SetTexts()
        {
            labelChooseDataSource.Text = UserTexts.Resources.PleaseChooseADatasource;
            labelChooseBusinessUnit.Text = UserTexts.Resources.PleaseChooseABusinessUnit;
            labelStatusText.Text = UserTexts.Resources.SearchingForDataSourcesTreeDots;
            labelLogOn.Text = UserTexts.Resources.PleaseEnterYourLogonCredentials;
            labelLoginName.Text = UserTexts.Resources.LoginNameColon;
            labelPassword.Text = UserTexts.Resources.PasswordColon;
            tabPageWindowsDataSources.Text = UserTexts.Resources.WindowsLogon;
            tabPageApplicationDataSources.Text = UserTexts.Resources.ApplicationLogon;
            buttonBusinessUnitsCancel.Text = UserTexts.Resources.Cancel;
            buttonBusinessUnitsOK.Text = UserTexts.Resources.Ok;
            buttonDataSourcesListCancel.Text = UserTexts.Resources.Cancel;
            buttonDataSourceListOK.Text = UserTexts.Resources.Ok;
            buttonLogOnCancel.Text = UserTexts.Resources.Cancel;
            buttonLogOnOK.Text = UserTexts.Resources.Ok;
        }

        /// <summary>
        /// Synchronizes the pre-defined and the loaded raptor application functions.
        /// </summary>
        private static void SynchronizeRaptorFunctions()
        {
            //RaptorApplicationFunctionsSynchronizer raptorSynchronizer = new RaptorApplicationFunctionsSynchronizer(new RepositoryFactory(), UnitOfWorkFactory.Current);
            //raptorSynchronizer.DigestApplicationFunctions();

            // TODO: Load the AP functions all checking goes here
            PermissionService.Instance().StartPermissionService();
        }

        /// <summary>
        /// Do an authentication.
        /// </summary>
        private void Authenticate(TeleoptiCccLogOnService service)
        {
            //Log on Step 1
            ICollection<DataSourceDto> dataSourceCollection = service.GetDataSources();

            _logonableWindowsDataSources = new List<DataSourceDto>();
            _availableApplicationDataSources = new List<DataSourceDto>();
            if (dataSourceCollection != null)
            {
                foreach (DataSourceDto dataSourceDto in dataSourceCollection)
                {
                    if (dataSourceDto.AuthenticationTypeOptionDto == AuthenticationTypeOptionDto.Windows)
                        _logonableWindowsDataSources.Add(dataSourceDto);
                    else
                    {
                        _availableApplicationDataSources.Add(dataSourceDto);
                    }
                }
            }

            NextState();

            // set tab pages if datasource list count equals 0
            if (_logonableWindowsDataSources.Count == 0)
            {
                AuthenticationType = AuthenticationTypeOptionDto.Application;
                tabControlChooseDataSource.TabPages.Remove(tabPageWindowsDataSources);
            }

            if (_availableApplicationDataSources.Count == 0)
            {
                AuthenticationType = AuthenticationTypeOptionDto.Windows;
                tabControlChooseDataSource.TabPages.Remove(tabPageApplicationDataSources);
            }

            // set logon for different scenarios
            if (_logonableWindowsDataSources.Count == 0 && _availableApplicationDataSources.Count == 0)
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                                            string.Concat(
                                                                UserTexts.Resources.
                                                                    NoAvailableDataSourcesHasBeenFound, "  "),
                                                            UserTexts.Resources.AuthenticationError,
                                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
                                                            MessageBoxDefaultButton.Button1,
                                                            (RightToLeft == RightToLeft.Yes)
                                                                ? MessageBoxOptions.RtlReading |
                                                                  MessageBoxOptions.RightAlign
                                                                : 0);
                _cancelLogOn = true;
            }
            else if (_logonableWindowsDataSources.Count == 1 && _availableApplicationDataSources.Count == 0)
            {
                _choosenDataSource = _logonableWindowsDataSources[0];
                ChooseBusinessUnit();
            }
            else if (_logonableWindowsDataSources.Count == 0 && _availableApplicationDataSources.Count == 1)
            {
                _choosenDataSource = _availableApplicationDataSources[0];
                ApplicationLogOn();
                ChooseBusinessUnit();
            }
            else
            {
                ChooseDataSource();
                ApplicationLogOn();
                ChooseBusinessUnit();
            }
        }

        private void ApplicationLogOn()
        {
            if (_cancelLogOn)
                return;

            if (AuthenticationType == AuthenticationTypeOptionDto.Application)
            {
                ActivatePanel(panelLogin);
                CancelButton = buttonLogOnCancel;
                AcceptButton = buttonLogOnOK;
                textBoxLogOnName.Focus();

                if (!string.IsNullOrEmpty(_logOnDetails.UserName)&& !string.IsNullOrEmpty(_logOnDetails.Password))
                {
                    signOn();
                    while (!_dataSourceLogonable && !_cancelLogOn)
                    {
                        _choosenDataSource = null;
                        _logOnDetails.UserName = string.Empty;
                        _logOnDetails.Password = string.Empty;
                        _logOnDetails.DataSource = null;
                        ChooseDataSource();
                        ApplicationLogOn();
                    }
                }
                // waiting for users response
                while (!_dataSourceLogonable && !_cancelLogOn)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
                
                if (_cancelLogOn)
                    return;
                var currentHeader = Sdk.Client.AuthenticationSoapHeader.Current;
                currentHeader.UserName = _logOnDetails.UserName;
                currentHeader.Password = _logOnDetails.Password;
                currentHeader.BusinessUnit = Guid.Empty.ToString();
            }
        }

        private void ChooseDataSource()
        {
            if (_cancelLogOn)
                return;

            ActivatePanel(panelChooseDataSource);

            AcceptButton = buttonDataSourceListOK;
            CancelButton = buttonDataSourcesListCancel;

            listBoxWindowsDataSources.Sorted = true;
            listBoxWindowsDataSources.DataSource = null;
            listBoxWindowsDataSources.Items.Clear();
            listBoxWindowsDataSources.DisplayMember = "Name";
            listBoxWindowsDataSources.DataSource = _logonableWindowsDataSources;

            listBoxApplicationDataSources.Sorted = true;
            listBoxApplicationDataSources.DataSource = null;
            listBoxApplicationDataSources.Items.Clear();
            listBoxApplicationDataSources.DisplayMember = "Name";
            listBoxApplicationDataSources.DataSource = _availableApplicationDataSources;

            if (AuthenticationType == AuthenticationTypeOptionDto.Windows)
            {
                tabControlChooseDataSource.SelectedTab = tabPageWindowsDataSources;
                listBoxWindowsDataSources.Focus();
            }
            else if (AuthenticationType == AuthenticationTypeOptionDto.Application)
            {
                tabControlChooseDataSource.SelectedTab = tabPageApplicationDataSources;
                listBoxApplicationDataSources.Focus();
            }

            // waiting for users response
            while (_choosenDataSource == null && !_cancelLogOn)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }

            if (_cancelLogOn)
                return;
        }

        private void ChooseBusinessUnit()
        {
            if (_cancelLogOn)
                return;

            var currentHeader = Sdk.Client.AuthenticationSoapHeader.Current;
            if (AuthenticationType == AuthenticationTypeOptionDto.Windows)
            {
                currentHeader.DataSource = _choosenDataSource.Name;
                currentHeader.UserName = string.Empty;
                currentHeader.Password = string.Empty;
                currentHeader.BusinessUnit = Guid.Empty.ToString();
                currentHeader.UseWindowsIdentity = true;

                var resultDto =
                    SdkServiceHelper.LogOnServiceClient.LogOnWindowsUser(
                        _choosenDataSource);
                _businessUnitList = resultDto.BusinessUnitCollection;
            }

            if (_businessUnitList.Count == 0)
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                                            string.Concat(
                                                                UserTexts.Resources.
                                                                    NoAllowedBusinessUnitFoundInCurrentDatabase, "  "),
                                                            " ",
                                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
                                                            MessageBoxDefaultButton.Button1,
                                                            (RightToLeft == RightToLeft.Yes)
                                                                ? MessageBoxOptions.RtlReading |
                                                                  MessageBoxOptions.RightAlign
                                                                : 0);
                _cancelLogOn = true;
                return;
            }
            if (_businessUnitList.Count > 1)
            {
                ActivatePanel(panelChooseBusinessUnit);

                AcceptButton = buttonBusinessUnitsOK;
                CancelButton = buttonBusinessUnitsCancel;

                listBoxBusinessUnits.Sorted = true;
                listBoxBusinessUnits.DataSource = null;
                listBoxBusinessUnits.Items.Clear();
                listBoxBusinessUnits.DataSource = _businessUnitList;
                listBoxBusinessUnits.DisplayMember = "Name";

                panelChooseBusinessUnit.Focus();
                listBoxBusinessUnits.Focus();

                // waiting for users response
                while (_choosenBusinessUnit==null && !_cancelLogOn)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
                if (_cancelLogOn)
                    return;
            }
            else
            {
                _choosenBusinessUnit = _businessUnitList[0];
            }

            ActivatePanel(panelPicture);

            currentHeader.BusinessUnit = _choosenBusinessUnit.Id;
            SdkServiceHelper.LogOnServiceClient.SetBusinessUnit(_choosenBusinessUnit);
        }

        private void CreateSettings()
        {
            StateHolder.Instance.State.SessionScopeData.AssignAppSettings(_appSettings);
        }

        /// <summary>
        /// Loggeds the on person data loaded.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/2/2008
        /// </remarks>
        private bool LoggedOnPersonDataLoaded()
        {
            bool result = false;

            PersonDto loggedPerson = SdkServiceHelper.LogOnServiceClient.GetLoggedOnPerson();

            if (loggedPerson != null)
            {
                // Intialize State Holder
#if (DEBUG)
                try
                {
                    StateHolder.Initialize(new StateManager());
                }
                catch (SoapException e)
                {
                    Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    e.Message,
                    UserTexts.Resources.ErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, 0);
                    return false;
                }
#else
                StateHolder.Initialize(new StateManager());
#endif
                StateHolder.Instance.State.SetSessionData(new SessionData(loggedPerson, _choosenBusinessUnit, _choosenDataSource, _logOnDetails.Password));

                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.GetCultureInfo(
                        loggedPerson.CultureLanguageId.GetValueOrDefault(CultureInfo.CurrentCulture.LCID));

                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.GetCultureInfo(
                        loggedPerson.UICultureLanguageId.GetValueOrDefault(CultureInfo.CurrentUICulture.LCID));

                result = true;
            }

            return result;
        }

        private bool Authorize()
        {
            if (!PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAgentPortal))
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    string.Concat(UserTexts.Resources.YouAreNotAuthorizedToRunTheApplication, "  "),
                    UserTexts.Resources.AuthorizationFailure, MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, 0);
                return false;
            }

            return true;
        }

        private bool VerifyLicense()
        {
            bool result = false;

            LicenseVerificationResultDto licenseVerificationResultDto = SdkServiceHelper.LogOnServiceClient.VerifyLicense();

            if (licenseVerificationResultDto.IsValidLicenseFound)
            {
                result = true;
                MainScreen.LicenseHolderName = licenseVerificationResultDto.LicenseHolderName;
            }
            else
            {
                if (licenseVerificationResultDto.IsExceptionFound)
                {
                    foreach (FaultDto exception in licenseVerificationResultDto.ExceptionCollection)
                    {
                        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                            exception.Message,
                            UserTexts.Resources.TeleoptiLicenseException,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
                    }
                }
            }

            return result;
        }
    }
}