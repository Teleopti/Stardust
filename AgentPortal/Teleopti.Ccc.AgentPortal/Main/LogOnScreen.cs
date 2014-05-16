using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

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
        private ICollection<BusinessUnitDto> _businessUnitList;
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
            AuthenticationMessageHeader.DataSource = _choosenDataSource.Name;
            AuthenticationMessageHeader.UseWindowsIdentity = false;
            _choosenDataSource.Client = "MYTIME";
            _choosenDataSource.IpAddress = ipAdress();
            var resultDto =
                SdkServiceHelper.LogOnServiceClient.LogOnApplicationUser(_logOnDetails.UserName, _logOnDetails.Password,
                                                                         _choosenDataSource);
            _businessUnitList = resultDto.BusinessUnitCollection;
            if (resultDto.HasMessage)
            {
                MessageBoxHelper.ShowWarningMessage(this, resultDto.Message, UserTexts.Resources.LogOn);
            }

            _dataSourceLogonable = resultDto.Successful;
        }

        private string ipAdress()
        {
            var ips = Dns.GetHostEntry(Dns.GetHostName());
            var ip = "";
            foreach (var adress in ips.AddressList)
            {
                if (adress.AddressFamily == AddressFamily.InterNetwork)
                    ip = adress.ToString();
            }
            return ip;
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
        private void Authenticate(ITeleoptiCccLogOnService service)
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
                MessageBoxHelper.ShowWarningMessage(this, UserTexts.Resources.NoAvailableDataSourcesHasBeenFound, UserTexts.Resources.AuthenticationError);
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
                _choosenDataSource.Client = "MYTIME";
                _choosenDataSource.IpAddress = ipAdress();
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
                AuthenticationMessageHeader.UserName = _logOnDetails.UserName;
                AuthenticationMessageHeader.Password = _logOnDetails.Password;
                AuthenticationMessageHeader.BusinessUnit = Guid.Empty;
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

            if (AuthenticationType == AuthenticationTypeOptionDto.Windows)
            {
                AuthenticationMessageHeader.DataSource = _choosenDataSource.Name;
                AuthenticationMessageHeader.UserName = string.Empty;
                AuthenticationMessageHeader.Password = string.Empty;
                AuthenticationMessageHeader.BusinessUnit = Guid.Empty;
                AuthenticationMessageHeader.UseWindowsIdentity = true;
                _choosenDataSource.Client = "MYTIME";
                _choosenDataSource.IpAddress = ipAdress();
                var resultDto =
                    SdkServiceHelper.LogOnServiceClient.LogOnWindowsUser(
                        _choosenDataSource);
                _businessUnitList = resultDto.BusinessUnitCollection;
            }

            if (_businessUnitList.Count == 0)
            {
            	MessageBoxHelper.ShowWarningMessage(this, UserTexts.Resources.NoAllowedBusinessUnitFoundInCurrentDatabase,
            	                                    UserTexts.Resources.AuthenticationError);
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
                _choosenBusinessUnit = _businessUnitList.First();
            }

            ActivatePanel(panelPicture);

            AuthenticationMessageHeader.BusinessUnit = _choosenBusinessUnit.Id.GetValueOrDefault();
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
                catch (FaultException e)
                {
                    MessageBoxHelper.ShowWarningMessage(this, e.Message, UserTexts.Resources.ErrorMessage);
                    return false;
                }
#else
                StateHolder.Initialize(new StateManager());
#endif
                StateHolder.Instance.State.SetSessionData(new SessionData(loggedPerson, _choosenBusinessUnit, _choosenDataSource, _logOnDetails.Password));

                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.GetCultureInfo(
						loggedPerson.CultureLanguageId.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)).FixPersianCulture();

                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.GetCultureInfo(
						loggedPerson.UICultureLanguageId.GetValueOrDefault(CultureInfo.CurrentUICulture.LCID)).FixPersianCulture();

                result = true;
            }

            return result;
        }

        private bool Authorize()
        {
            if (!PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAgentPortal))
            {
            	MessageBoxHelper.ShowWarningMessage(this, UserTexts.Resources.YouAreNotAuthorizedToRunTheApplication,
            	                                    UserTexts.Resources.AuthorizationFailure);
                return false;
            }

            return true;
        }

        private bool VerifyLicense()
        {
            bool result = false;

			var currentBusinessUnit = AuthenticationMessageHeader.BusinessUnit;
			AuthenticationMessageHeader.BusinessUnit = Guid.Empty;
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
                        MessageBoxHelper.ShowWarningMessage(this, exception.Message, UserTexts.Resources.TeleoptiLicenseException);
                    }
                }
            }

			AuthenticationMessageHeader.BusinessUnit = currentBusinessUnit;
            return result;
        }
    }
}