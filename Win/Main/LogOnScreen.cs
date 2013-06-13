using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class LogOnScreen : Form, ILicenseFeedback
	{
		private readonly ILogOnOff _logOnOff;
		private readonly IDataSourceHandler _dataSourceHandler;

		private enum LoadingState
		{
			Initializing = 0,
			LoggingIn = 1,
			Loading = 2,
			Verifying = 3,
			Ready = 4
		}

		private LoadingState _loadingState = LoadingState.Initializing;
		private readonly TypedBindingCollection<DataSourceContainer> _logonableWindowsDataSources = new TypedBindingCollection<DataSourceContainer>();
		private readonly TypedBindingCollection<DataSourceContainer> _availableApplicationDataSources = new TypedBindingCollection<DataSourceContainer>();
		private DataSourceContainer _choosenDataSource;
		private AuthenticationTypeOption _authenticationType = AuthenticationTypeOption.Windows;
		private delegate bool LoadingFunction();
		private readonly IEnumerable<LoadingFunction> _loadingFunctions;
		private readonly IEnumerable<LoadingFunction> _initializeFunctions;
		private readonly WindowsAppDomainPrincipalContext _principalContext = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory());
		private IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly ILogonLogger _logonLogger;

		public LogOnScreen()
		{
			InitializeComponent();
			label1.Text = string.Concat("Build ", Application.ProductVersion);

			SetTexts();

			_loadingFunctions = new List<LoadingFunction>
                                    {
                                        InitializeAndCheckStateHolder,
                                        FindAvailableDataSources,
                                        CheckAndReportInvalidDataSources,
                                        NextState,
                                        RemoveEmptyTabPages,
                                        CheckIfNoDataSources,
                                        LogOnWhenOneWindowsDataSource,
                                        LogOnWhenOneApplicationDataSource,
                                        LogOnWhenMoreThanOneDataSource,
                                    };

			_initializeFunctions = new List<LoadingFunction>
                                       {
                                           SetCultureInformation,
                                           NextState,
                                           InitializeLicense,
                                           NextState,
                                           CheckRaptorApplicationFunctions,
                                       };
		}

		public LogOnScreen(ILogOnOff logOnOff, IDataSourceHandler dataSourceHandler, IRoleToPrincipalCommand roleToPrincipalCommand, ILogonLogger logonLogger)
			: this()
		{
			_logOnOff = logOnOff;
			_dataSourceHandler = dataSourceHandler;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_logonLogger = logonLogger;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			ActivatePanel(panelPicture);
			ViewState();

			Refresh();

			foreach (LoadingFunction loadingFunction in _loadingFunctions)
			{
				if (IsDisposed) return;
				if (!loadingFunction())
				{
					DialogResult = DialogResult.Cancel;
					Close();
					return;
				}
			}
			DialogResult = DialogResult.None;
		}

		private void InitializeStuff()
		{
			foreach (LoadingFunction loadingFunction in _initializeFunctions)
			{
				if (IsDisposed) return;
				if (!loadingFunction())
				{
					DialogResult = DialogResult.Cancel;
					Close();
					return;
				}
			}

			LogOnMatrix.SynchronizeAndLoadMatrixReports(this);

			if (LogOnAuthorize.Authorize())
				DialogResult = DialogResult.OK;
			else
				DialogResult = DialogResult.Cancel;

			_loadingState = LoadingState.LoggingIn;

			NextState();

			Visible = false;
		}

		private bool InitializeLicense()
		{
			var unitOfWorkFactory = _choosenDataSource.DataSource.Application;
			var verifier = new LicenseVerifier(this, unitOfWorkFactory, new LicenseRepository(unitOfWorkFactory));
			ILicenseService licenseService = verifier.LoadAndVerifyLicense();
			if (licenseService == null)
			{
				return false;
			}
			LicenseProvider.ProvideLicenseActivator(licenseService);
			return CheckStatusOfLicense(licenseService);
		}

		private bool CheckStatusOfLicense(ILicenseService licenseService)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new LicenseStatusRepository(UnitOfWorkFactory.Current);
				// if something goes wrong here the document is corrupt, handle that in some way ??
				var status = rep.LoadAll().First();
				var licenseStatus = new LicenseStatusXml(XDocument.Parse(status.XmlString));
				if (licenseStatus.StatusOk && licenseStatus.AlmostTooMany)
				{
					Warning(getAlmostTooManyAgentsWarning(licenseStatus.NumberOfActiveAgents, licenseService));
				}
				if (!licenseStatus.StatusOk && licenseStatus.DaysLeft > 0)
				{
					Warning(getLicenseIsOverUsedWarning(licenseService, licenseStatus));
				}
				if (!licenseStatus.StatusOk && licenseStatus.DaysLeft < 1)
				{
					Error(getTooManyAgentsExplanation(licenseService, UnitOfWorkFactory.Current.Name, licenseStatus.NumberOfActiveAgents));
					return false;
				}
			}
			return true;
		}

		private static string getLicenseIsOverUsedWarning(ILicenseService licenseService, ILicenseStatusXml licenseStatus)
		{
			int maxLicensed;
			if (licenseService.LicenseType.Equals(LicenseType.Agent))
				maxLicensed = licenseService.MaxActiveAgents;
			else
				maxLicensed = licenseService.MaxSeats;

			return String.Format(CultureInfo.CurrentCulture, Resources.TooManyAgentsIsUsedWarning,
								 licenseStatus.NumberOfActiveAgents, maxLicensed, licenseStatus.DaysLeft);
		}

		private static string getAlmostTooManyAgentsWarning(int numberOfActiveAgents, ILicenseService licenseService)
		{
			string warningMessage;
			if (licenseService.LicenseType.Equals(LicenseType.Agent))
			{
				warningMessage = String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManyActiveAgents,
								  numberOfActiveAgents, licenseService.MaxActiveAgents);
			}
			else
			{
				warningMessage = String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManySeats,
											   licenseService.MaxSeats);

			}
			return warningMessage;
		}

		private static string getTooManyAgentsExplanation(ILicenseService licenseService, string dataSourceName, int numberOfActiveAgents)
		{
			if (licenseService.LicenseType == LicenseType.Agent)
				return dataSourceName + "\r\n" +
					   String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents,
									 numberOfActiveAgents, licenseService.MaxActiveAgents);
			return dataSourceName + "\r\n" + String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats,
														   licenseService.MaxSeats);
		}

		private static bool SetCultureInformation()
		{
			if (TeleoptiPrincipal.Current.Regional != null)
			{
				Thread.CurrentThread.CurrentCulture =
					TeleoptiPrincipal.Current.Regional.Culture;
				Thread.CurrentThread.CurrentUICulture =
					TeleoptiPrincipal.Current.Regional.UICulture;
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons)")]
		private bool InitializeAndCheckStateHolder()
		{
			if (!LogOnInitializeStateHolder.InitializeStateHolder())
			{
				ShowInTaskbar = true;
				MessageBox.Show(this,
								string.Format(CultureInfo.CurrentCulture,
											  "The system configuration could not be loaded from the server. Review error message and log files to troubleshoot this error.\n\n{0}",
											  LogOnInitializeStateHolder.ErrorMessage),
								"Configuration error", MessageBoxButtons.OK);
				return false;
			}
			if (!string.IsNullOrEmpty(LogOnInitializeStateHolder.WarningMessage))
			{
				ShowInTaskbar = true;
				MessageBox.Show(this, LogOnInitializeStateHolder.WarningMessage, "Configuration warning", MessageBoxButtons.OK);
				ShowInTaskbar = false;
			}
			return true;
		}

		private void tabControlChooseDataSource_SelectedIndexChanged(object sender, EventArgs e)
		{
			setAuthenticationAndFocus();
		}

		private void setAuthenticationAndFocus()
		{
			if (tabControlChooseDataSource.SelectedTab.Equals(tabPageWindowsDataSources))
			{
				_authenticationType = AuthenticationTypeOption.Windows;
				listBoxWindowsDataSources.Focus();
			}
			else
			{
				_authenticationType = AuthenticationTypeOption.Application;
				listBoxApplicationDataSources.Focus();
			}
		}

		private void buttonLogOnOK_Click(object sender, EventArgs e)
		{
			string logOnName = textBoxLogOnName.Text;
			if (!string.IsNullOrEmpty(logOnName))
			{
				string password = textBoxPassword.Text;
				
				var authenticationResult = _choosenDataSource.LogOn(textBoxLogOnName.Text, password);

				if (authenticationResult.HasMessage)
					MessageDialogs.ShowError(this, string.Concat(authenticationResult.Message, "  "), Resources.LogOn);
				if (authenticationResult.Successful)
				{
					_choosenDataSource.User.ApplicationAuthenticationInfo.Password = password; //To use for silent background log on

					ChooseBusinessUnit();
				}
				else
				{
					var model = new LoginAttemptModel
					{
						ClientIp = ipAdress(),
						Client = "WIN",
						UserCredentials = _choosenDataSource.LogOnName,
						Provider = _choosenDataSource.AuthenticationTypeOption.ToString(),
						Result = "LogonFailed"
					};

					_logonLogger.SaveLogonAttempt(model, _choosenDataSource.DataSource.Application);
				}
			}
		}

		private void buttonDataSourcesListOK_Click(object sender, EventArgs e)
		{
			ListBox listBox = _authenticationType == AuthenticationTypeOption.Windows
								  ? listBoxWindowsDataSources
								  : listBoxApplicationDataSources;
			if (SetDataSource((DataSourceContainer)((TupleItem)listBox.SelectedItem).ValueMember))
				ApplicationLogOn();
		}

		private bool SetDataSource(DataSourceContainer dataSourceContainer)
		{
			_choosenDataSource = dataSourceContainer;

			_principalContext.SetCurrentPrincipal(_choosenDataSource.User, _choosenDataSource.DataSource, null);
			return true;
		}

		private void buttonBusinessUnitsOK_Click(object sender, EventArgs e)
		{
			SetBusinessUnit((IBusinessUnit)listBoxBusinessUnits.SelectedItem, _choosenDataSource.AvailableBusinessUnitProvider, _choosenDataSource);
		}

		private void buttonLogOnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
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
		}

		/// <summary>
		/// Moves to the next loading state.
		/// </summary>
		private bool NextState()
		{
			_loadingState++;
			ViewState();
			return true;
		}

		private void ViewState()
		{
			ShowInTaskbar = false;

			switch (_loadingState)
			{
				case LoadingState.Initializing:
					labelStatusText.Text = Resources.InitializingTreeDots;
					break;
				case LoadingState.LoggingIn:
					ShowInTaskbar = true;
					SuspendLayout();
					pictureBoxStep2.Dock = DockStyle.Fill;
					pictureBoxStep2.Visible = true;
					pictureBoxStep1.Visible = false;
					ResumeLayout();
					labelStatusText.Text = Resources.LoggingOnTreeDots;
					break;
				case LoadingState.Loading:
					labelStatusText.Text = Resources.LoadingDataTreeDots;
					break;
				case LoadingState.Verifying:
					SuspendLayout();
					pictureBoxStep3.Dock = DockStyle.Fill;
					pictureBoxStep3.Visible = true;
					pictureBoxStep2.Visible = false;
					ResumeLayout();
					labelStatusText.Text = Resources.VerifyingPermissionsTreeDots;
					break;
				case LoadingState.Ready:
					break;
			}

			label1.BringToFront();
			Refresh();
		}

		private void SetTexts()
		{
			labelChooseDataSource.Text = Resources.PleaseChooseADatasource;
			labelChooseBusinessUnit.Text = Resources.PleaseChooseABusinessUnit;
			labelStatusText.Text = Resources.SearchingForDataSourcesTreeDots;
			labelLogOn.Text = Resources.PleaseEnterYourLogonCredentials;
			labelLoginName.Text = Resources.LoginNameColon;
			labelPassword.Text = Resources.PasswordColon;
			tabPageWindowsDataSources.Text = Resources.WindowsLogon;
			tabPageApplicationDataSources.Text = Resources.ApplicationLogon;
			buttonBusinessUnitsCancel.Text = Resources.Cancel;
			buttonBusinessUnitsOK.Text = Resources.Ok;
			buttonDataSourcesListCancel.Text = Resources.Cancel;
			buttonDataSourceListOK.Text = Resources.Ok;
			buttonLogOnCancel.Text = Resources.Cancel;
			buttonLogOnOK.Text = Resources.Ok;
		}

		private bool LogOnWhenMoreThanOneDataSource()
		{
			if (MoreThanOneDataSource())
			{
				ChooseDataSource();
			}
			return true;
		}

		private bool LogOnWhenOneApplicationDataSource()
		{
			if (OneApplicationDataSource())
			{
				_choosenDataSource = _availableApplicationDataSources[0];
				ApplicationLogOn();
			}
			return true;
		}

		private bool LogOnWhenOneWindowsDataSource()
		{
			if (OneWindowsDataSource())
			{
				_choosenDataSource = _logonableWindowsDataSources[0];
				ChooseBusinessUnit();
			}
			return true;
		}

		private bool CheckIfNoDataSources()
		{
			if (NoDataSources())
			{
				MessageDialogs.ShowError(this, string.Concat(Resources.NoAvailableDataSourcesHasBeenFound, "  "), Resources.LogOn);
				return false;
			}
			return true;
		}

		private bool MoreThanOneDataSource()
		{
			return _logonableWindowsDataSources.Count + _availableApplicationDataSources.Count > 1;
		}

		private bool OneApplicationDataSource()
		{
			return _logonableWindowsDataSources.Count == 0 && _availableApplicationDataSources.Count == 1;
		}

		private bool OneWindowsDataSource()
		{
			return _logonableWindowsDataSources.Count == 1 && _availableApplicationDataSources.Count == 0;
		}

		private bool NoDataSources()
		{
			return _logonableWindowsDataSources.Count == 0 && _availableApplicationDataSources.Count == 0;
		}

		private bool RemoveEmptyTabPages()
		{
			if (_logonableWindowsDataSources.Count == 0)
			{
				_authenticationType = AuthenticationTypeOption.Application;
				tabControlChooseDataSource.TabPages.Remove(tabPageWindowsDataSources);
			}

			if (_availableApplicationDataSources.Count == 0)
			{
				_authenticationType = AuthenticationTypeOption.Windows;
				tabControlChooseDataSource.TabPages.Remove(tabPageApplicationDataSources);
			}
			return true;
		}

		private bool FindAvailableDataSources()
		{
			using (PerformanceOutput.ForOperation("Finding possible datasources to log on to"))
			{
				// fill up datasources lists
				var logonableDataSources = new List<DataSourceContainer>();
				foreach (IDataSourceProvider dataSourceProvider in _dataSourceHandler.DataSourceProviders())
				{
					logonableDataSources.AddRange(dataSourceProvider.DataSourceList());
				}
				foreach (DataSourceContainer dataSource in logonableDataSources.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows))
				{
					_logonableWindowsDataSources.Add(dataSource);
				}
				foreach (DataSourceContainer dataSource in logonableDataSources.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application))
				{
					_availableApplicationDataSources.Add(dataSource);
				}
			}
			return true;
		}

		private bool CheckRaptorApplicationFunctions()
		{
			var repositoryFactory = new RepositoryFactory();
			var raptorSynchronizer = new RaptorApplicationFunctionsSynchronizer(repositoryFactory, UnitOfWorkFactory.Current);

			var result = raptorSynchronizer.CheckRaptorApplicationFunctions();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_roleToPrincipalCommand.Execute(TeleoptiPrincipal.Current, uow, repositoryFactory.CreatePersonRepository(uow));
			}

			if (result.Result)
				return true;

			ShowInTaskbar = true;
			string message =
				BuildApplicationFunctionWarningMessage(result);
			DialogResult answer = MessageBox.Show(
					this,
					message,
					Resources.VerifyingPermissionsTreeDots,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button1,
					0);

			return (answer == DialogResult.Yes);
		}

		private static string BuildApplicationFunctionWarningMessage(CheckRaptorApplicationFunctionsResult result)
		{
			string message = string.Empty;
			// Added
			if (result.AddedFunctions.Any())
			{
				var p = result.AddedFunctions.Select(f => f.FunctionPath);
				var appFunctionsText = string.Join(", ", p);

				message = "The following Application Function(s) has been added recently in code but not found in the database: "
							+ appFunctionsText
							+ "\nYou can continue, but the program might be unstable. Continue?";
				return message;
			}

			// Deleted
			if (result.DeletedFunctions.Any())
			{
				var p = result.DeletedFunctions.Select(f => f.FunctionPath);
				var appFunctionsText = string.Join(", ", p);

				message = "The following Application Function(s) has been removed recently from code but still exists in the database: "
						   + appFunctionsText
						   + "\nYou can continue, but the program might be unstable. Continue?";
			}

			return message;
		}

		private bool CheckAndReportInvalidDataSources()
		{
			var notAvailableDataSources = _dataSourceHandler.AvailableDataSourcesProvider().UnavailableDataSources().Select(s => s.Application.Name);
			if (!notAvailableDataSources.IsEmpty())
			{
				string msg = "The following data source(s) is currently not available:";
				foreach (string source in notAvailableDataSources)
				{
					msg += "\n\t- " + source;
				}
				msg += "\nThe data source server is probably down or the connection string is invalid.";
				MessageDialogs.ShowWarning(this, msg, Resources.LogOn);
			}
			return true;
		}

		private void ApplicationLogOn()
		{
			if (_authenticationType == AuthenticationTypeOption.Application)
			{
				ActivatePanel(panelLogin);
				CancelButton = buttonLogOnCancel;
				AcceptButton = buttonLogOnOK;

				textBoxLogOnName.Focus();
			}
			else
			{
				ChooseBusinessUnit();
			}
		}

		private void ChooseDataSource()
		{
			ActivatePanel(panelChooseDataSource);

			AcceptButton = buttonDataSourceListOK;
			CancelButton = buttonDataSourcesListCancel;

			listBoxApplicationDataSources.Sorted = true;
			listBoxApplicationDataSources.Items.Clear();
			listBoxApplicationDataSources.DisplayMember = "Text";
			_availableApplicationDataSources.ForEach(
				i => listBoxApplicationDataSources.Items.Add(new TupleItem(i.DataSource.Application.Name, i)));
			if (listBoxApplicationDataSources.Items.Count > 0)
			{
				listBoxApplicationDataSources.SelectedIndex = 0;
				tabControlChooseDataSource.SelectedTab = tabPageApplicationDataSources;
			}

			listBoxWindowsDataSources.Sorted = true;
			listBoxWindowsDataSources.Items.Clear();
			listBoxWindowsDataSources.DisplayMember = "Text";
			_logonableWindowsDataSources.ForEach(
				i => listBoxWindowsDataSources.Items.Add(new TupleItem(i.DataSource.Application.Name, i)));
			if (listBoxWindowsDataSources.Items.Count > 0)
			{
				listBoxWindowsDataSources.SelectedIndex = 0;
				tabControlChooseDataSource.SelectedTab = tabPageWindowsDataSources;
			}

			setAuthenticationAndFocus();
		}

		private void ChooseBusinessUnit()
		{
			var provider = _choosenDataSource.AvailableBusinessUnitProvider;
			IList<IBusinessUnit> buList = provider.AvailableBusinessUnits().ToList();
			if (buList.Count == 0)
			{
				MessageDialogs.ShowError(this, string.Concat(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, "  "), Resources.LogOn);
				return;
			}
			if (buList.Count > 1)
			{
				ActivatePanel(panelChooseBusinessUnit);

				AcceptButton = buttonBusinessUnitsOK;
				CancelButton = buttonBusinessUnitsCancel;

				listBoxBusinessUnits.Sorted = true;
				listBoxBusinessUnits.DataSource = null;
				listBoxBusinessUnits.Items.Clear();
				listBoxBusinessUnits.DataSource = buList;
				listBoxBusinessUnits.DisplayMember = "Name";

				panelChooseBusinessUnit.Focus();
				listBoxBusinessUnits.Focus();
			}
			else
				SetBusinessUnit(buList[0], provider, _choosenDataSource);
		}

		private void SetBusinessUnit(IBusinessUnit businessUnit, AvailableBusinessUnitsProvider provider, DataSourceContainer dataSourceContainer)
		{
			ActivatePanel(panelPicture);

			businessUnit = provider.LoadHierarchyInformation(businessUnit);

			_logOnOff.LogOn(dataSourceContainer.DataSource, dataSourceContainer.User, businessUnit);

			var model = new LoginAttemptModel
			{
				ClientIp = ipAdress(),
				Client = "WIN",
				UserCredentials = dataSourceContainer.LogOnName,
				Provider = dataSourceContainer.AuthenticationTypeOption.ToString(),
				Result = "LogonSuccess"
			};
			if (dataSourceContainer.User != null) model.PersonId = dataSourceContainer.User.Id;

			_logonLogger.SaveLogonAttempt(model, dataSourceContainer.DataSource.Application);

			StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption = _authenticationType;

			InitializeStuff();
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
		public void Warning(string warning)
		{
			ShowInTaskbar = true;
			MessageDialogs.ShowWarning(this, warning, Resources.LogOn);
			ShowInTaskbar = false;

			DialogResult = DialogResult.None;
		}

		public void Error(string error)
		{
			ShowApplyLicenseDialogAndExit(error);
		}

		private void ShowApplyLicenseDialogAndExit(string explanation)
		{
			ApplyLicense applyLicense = new ApplyLicense(explanation, _choosenDataSource.DataSource.Application);
			applyLicense.ShowDialog(this);
			Application.Exit();
		}

		private void listBoxApplicationDataSources_DoubleClick(object sender, EventArgs e)
		{
			if (SetDataSource((DataSourceContainer)((TupleItem)listBoxApplicationDataSources.SelectedItem).ValueMember))
			{
				ApplicationLogOn();
			}
		}
		private void listBoxWindowsDataSources_DoubleClick(object sender, EventArgs e)
		{
			if (SetDataSource((DataSourceContainer)((TupleItem)listBoxWindowsDataSources.SelectedItem).ValueMember))
			{
				ApplicationLogOn();
			}
		}
	}
}
