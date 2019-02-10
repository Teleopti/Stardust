using Autofac;
using EO.Base;
using EO.WebBrowser;
using log4net;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.SmartClientPortal.Shell.Win;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.OutlookControls.Workspaces;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using Teleopti.Ccc.Win.Main;
using Action = System.Action;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	/// <summary>
	/// Main application SmartClientShell view.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class SmartClientShellForm : BaseRibbonForm, IDummyInterface
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(SmartClientShellForm));
		private readonly IComponentContext _container;

		private readonly SystemCheckerValidator _systemChecker;
		private readonly OutlookPanelContentWorker _outlookPanelContentWorker;
		private readonly PortalSettings _portalSettings;
		private readonly IToggleManager _toggleManager;
		readonly int homeCommand = CommandIds.RegisterUserCommand("StartPage");
		private bool showCustomerWebMenu = true;

		protected SmartClientShellForm()
		{
			using (PerformanceOutput.ForOperation("SmartClientPortal ctor"))
			{
				InitializeComponent();

				if (!DesignMode)
				{
					SetTexts();
					ribbonControlAdv1.MenuButtonText = LanguageResourceHelper.Translate(ribbonControlAdv1.MenuButtonText);
					ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlAdv1BeforeContextMenuOpen;
				}
			}
			KeyPreview = true;
			KeyDown += formKeyDown;
			KeyPress += Form_KeyPress;

			EO.Base.Runtime.Exception += handlingEoRuntimeErrors;
		}

		void formKeyDown(object sender, KeyEventArgs e)
		{
			// key shortcuts
			if (e.Modifiers == Keys.Control)
			{
				if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
					openOptionsDialog();
				else if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
					startModule(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
				else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
					startModule(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage);
				else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
					startModule(DefinedRaptorApplicationFunctionPaths.Shifts);
				else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4)
					startModule(DefinedRaptorApplicationFunctionPaths.OpenSchedulePage);
				else if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5)
					startModule(DefinedRaptorApplicationFunctionPaths.OpenIntradayPage);
				else if (e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6)
					startModule(DefinedRaptorApplicationFunctionPaths.AccessToReports);
				else if (e.KeyCode == Keys.D7 || e.KeyCode == Keys.NumPad7)
					startModule(DefinedRaptorApplicationFunctionPaths.OpenBudgets);
				else if (e.KeyCode == Keys.D8 || e.KeyCode == Keys.NumPad8)
					startModule(DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager);
				else if (e.KeyCode == Keys.D9 || e.KeyCode == Keys.NumPad9)
					startModule(DefinedRaptorApplicationFunctionPaths.PayrollIntegration);
			}

			if (e.KeyValue.Equals(32))
			{
				e.Handled = true;
			}
		}

		void Form_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar.Equals((Char)Keys.Space))
			{
				e.Handled = true;
			}
		}

		static void ribbonControlAdv1BeforeContextMenuOpen(object sender, ContextMenuEventArgs e)
		{
			e.Cancel = true;
		}

		public SmartClientShellForm(IComponentContext container) : this()
		{
			_container = container;

			//This is here instead of in the constructor because this will be created by ObjectBuilder instead of AutoFac
			_systemChecker = _container.Resolve<SystemCheckerValidator>();
			_outlookPanelContentWorker = _container.Resolve<OutlookPanelContentWorker>();
			_portalSettings = _container.Resolve<PortalSettings>();
			_toggleManager = _container.Resolve<IToggleManager>();
		}

		void toolStripButtonHelpClick(object sender, EventArgs e)
		{
			ViewBase.ShowHelp(this, false);
		}

		private void toolStripButtonAboutClick(object sender, EventArgs e)
		{
			var about = new About();
			about.ShowDialog();
			if (about.ProductActivationKeyWasApplied)
			{
				Close();
			}
		}

		private void toolStripButtonSystemOptionsClick(object sender, EventArgs e)
		{
			openOptionsDialog();
		}

		private void toolStripButtonSystemExitClick(object sender, EventArgs e)
		{
			Close();
		}

		public StatusStrip MainStatusStrip
		{
			get { return _mainStatusStrip; }
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.G && e.Shift && e.Alt)
			{
				toolStripStatusLabelRoger65.Visible = true;
				roger65(MemoryCounter.DefaultInstance().CurrentMemoryConsumptionString());
			}
			if (e.KeyCode == Keys.M && e.Shift && e.Alt)
			{
				TestMode.Micke = true;
			}
			if (e.KeyCode == Keys.I && e.Shift && e.Alt)
			{
				SikuliHelper.SetInteractiveMode(true);
			}
			if (e.KeyCode == Keys.N && e.Shift && e.Alt)
			{
				SikuliHelper.SetInteractiveMode(false);
			}
			if (e.KeyCode == Keys.V && e.Shift && e.Alt)
			{
				SikuliHelper.EnterValidator(this);
			}
			base.OnKeyDown(e);
		}

		private void startModule(string modulePath)
		{
			foreach (var modulePanelItem in outlookBar1.Items)
			{
				if (modulePanelItem.Tag.Equals(modulePath))
				{
					outlookBar1.SelectItem(modulePanelItem);
					return;
				}
			}
		}

		private void startFirstEnabledModule()
		{
			foreach (var modulePanelItem in outlookBar1.Items)
			{
				if (modulePanelItem.ItemEnabled)
				{
					outlookBar1.SelectItem(modulePanelItem);
					return;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void smartClientShellFormLoad(object sender, EventArgs e)
		{
			Enabled = false;
			Cursor = Cursors.WaitCursor;
			toolStripStatusLabelSpring.Text = LanguageResourceHelper.Translate("XXLoadingThreeDots");
			_mainStatusStrip.Refresh();
			var identity = (ITeleoptiIdentityWithUnsafeBusinessUnit)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity;
			var loggedOnBu = identity.BusinessUnit();
			Text = UserTexts.Resources.TeleoptiRaptorColonMainNavigation + @" " + loggedOnBu.Name;

			toolStripStatusLabelLicense.Text = toolStripStatusLabelLicense.Text + ApplicationTextHelper.LicensedToCustomerText;
			toolStripStatusLabelLoggedOnUser.Text = toolStripStatusLabelLoggedOnUser.Text +
													ApplicationTextHelper.LoggedOnUserText;
			goToPublicPage(false);

			setNotifyData(_systemChecker.IsOk());

			loadOutLookBar();

			initializeSmartPartInvoker();

			roger65(string.Empty);
			setPermissionOnToolStripButtonControls();

			backStage1.Controls.Remove(backStageButtonSignCustomerWeb);

			_container.Resolve<IHangfireClientStarter>().Start();
		}

		private void smartClientShellFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (!CloseAllOtherForms(this))
			{
				e.Cancel = true;
				return; // a form was canceled
			}

			try
			{
				persistSetting();
			}
			catch (DataSourceException dataSourceException)
			{
				_logger.Error("An error occurred when trying to save settings on exit.", dataSourceException);
			}
		}

		private void toolStripButtonPermissonsClick(object sender, EventArgs e)
		{
			if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				Process.Start(buildWfmUri("WFM/#/permissions").ToString());
			}
		}

		private void persistSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonalSettingDataRepository(uow).PersistSettingValue(_portalSettings);
				uow.PersistAll();
			}
		}

		private void roger65(string message)
		{
			toolStripStatusLabelRoger65.Text = message;
			toolStripStatusLabelRoger65.ForeColor = Color.Red;
		}

		private void setPermissionOnToolStripButtonControls()
		{
			var authorization = PrincipalAuthorization.Current_DONTUSE();
			backStageButtonPermissions.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions);
			backStageButtonOptions.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
			var type = LogonPresenter.AuthenticationTypeOption;
			backStageButtonMyProfile.Enabled = type.Equals(AuthenticationTypeOption.Application);
		}

		private void loadOutLookBar()
		{
			var authorization = PrincipalAuthorization.Current_DONTUSE();
			IEnumerable<IApplicationFunction> modules = authorization.GrantedFunctions().FilterBySpecification(new ModuleSpecification());

			IList<ModulePanelItem> modulePanelItems = new List<ModulePanelItem>();
			foreach (IApplicationFunction module in modules.OrderBy(m => m.SortOrder.GetValueOrDefault(1000000)))
			{
				if (module.IsPreliminary)
					continue;

				var outlookBarSmartPartInfo = new OutlookBarInfo();
				string name = module.FunctionDescription;
				if (name.StartsWith("xx", StringComparison.OrdinalIgnoreCase))
				{
					string resName = UserTexts.Resources.ResourceManager.GetString(name.Substring(2));
					if (!string.IsNullOrEmpty(resName))
						name = resName;
				}
				outlookBarSmartPartInfo.Title = name;
				outlookBarSmartPartInfo.EventTopicName = module.FunctionPath;
				outlookBarSmartPartInfo.Enable = authorization.IsPermitted(module.FunctionPath);

				switch (module.FunctionPath)
				{
					case DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage:
						outlookBarSmartPartInfo.Icon = Resources.People_filled_space_32x32;
						if (_toggleManager.IsEnabled(Toggles.Wfm_PeopleWeb_PrepareForRelease_74903) && PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPeople))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewPeopleModule;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/people");
						}
						break;

					case DefinedRaptorApplicationFunctionPaths.OpenForecasterPage:
						outlookBarSmartPartInfo.Icon = Resources.Forecasts2_filled_32x32;
						if (_toggleManager.IsEnabled(Toggles.WFM_Forecaster_Preview_74801) &&
							PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebForecasts))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewForecasts;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/forecast");
						}
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenSchedulePage:
						outlookBarSmartPartInfo.Icon = Resources.Schedules_filled_space_32x32;
						if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebRequests))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewRequestsModule;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/requests");
						}
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenIntradayPage:
						outlookBarSmartPartInfo.Icon = Resources.Intraday_filled_space_32x32;
						break;
					case DefinedRaptorApplicationFunctionPaths.Shifts:
						outlookBarSmartPartInfo.Icon = Resources.Shifts_filled_space_32x32;
						break;
					case DefinedRaptorApplicationFunctionPaths.AccessToReports:
						outlookBarSmartPartInfo.Icon = Resources.Reports_filled_space_32x32;
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenOptionsPage:
						outlookBarSmartPartInfo.Icon = Resources.WFM_Teleopti_WFM_main_small;
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenBudgets:
						outlookBarSmartPartInfo.Icon = Resources.Budgets_filled_space_32x32;
						break;
					case DefinedRaptorApplicationFunctionPaths.PayrollIntegration:
						outlookBarSmartPartInfo.Icon = Resources.Payroll_filled_space_32x32;
						break;
					default:
						// add default image as a resource to your module.
						outlookBarSmartPartInfo.Icon = Resources.Performance_Manager_filled_space_32x32;
						break;
				}
				modulePanelItems.Add(new ModulePanelItem
				{
					ItemImage = outlookBarSmartPartInfo.Icon,
					ItemText = outlookBarSmartPartInfo.Title,
					ItemEnabled = outlookBarSmartPartInfo.Enable,
					Tag = outlookBarSmartPartInfo.EventTopicName,
					PreviewText = outlookBarSmartPartInfo.PreviewText,
					PreviewUrl = outlookBarSmartPartInfo.PreviewUrl
				});
			}

			outlookBar1.AddItems(modulePanelItems.ToArray());
		}

		private void initializeSmartPartInvoker()
		{
			var smartPartWorker = new SmartPartWorker(gridWorkspace);
			var smartPartCommand = new SmartPartCommand(smartPartWorker);
			SmartPartInvoker.SmartPartCommand = smartPartCommand;
			SmartPartEnvironment.MessageBroker = MessageBrokerInStateHolder.Instance;
			SmartPartEnvironment.SmartPartWorkspace = gridWorkspace;
		}

		private void showBalloon()
		{
			notifyIcon.ShowBalloonTip(100);
		}

		private void setNotifyData(bool isOk)
		{
			if (isOk)
			{
				notifyIcon.Icon = Resources.NotifyOK;
				notifyIcon.Text = UserTexts.Resources.CheckSystemOk;
				notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
				notifyIcon.BalloonTipTitle = UserTexts.Resources.CheckSystemOk;
				notifyIcon.BalloonTipText = UserTexts.Resources.CheckSystemOk;
			}
			else
			{
				notifyIcon.Icon = Resources.NotifyWarning;
				notifyIcon.Text = UserTexts.Resources.CheckSystemWarning;
				notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
				notifyIcon.BalloonTipTitle = UserTexts.Resources.CheckSystemWarning;
				notifyIcon.BalloonTipText = warningsAsOneString();
			}
		}

		private string warningsAsOneString()
		{
			var ret = string.Empty;
			foreach (var warning in _systemChecker.Result)
			{
				ret += warning + Environment.NewLine;
			}
			return ret;
		}

		private void notifyIconMouseClick(object sender, MouseEventArgs e)
		{
			showBalloon();
		}

		private void toolStripButtonMyProfileClick(object sender, EventArgs e)
		{
			try
			{
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settings = new OptionDialog(new OptionCore(new MyProfileSettingPagesProvider(_container)));
					settings.SetUnitOfWork(unitOfWork);
					settings.Page(typeof(ChangePasswordControl));
					settings.ShowDialog();
				}
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}

		}

		public void ModuleSelected(ModulePanelItem modulePanelItem)
		{
			if (modulePanelItem == null)
				return;

			_portalSettings.LastModule = modulePanelItem.Tag.ToString();
			SmartPartInvoker.ClearAllSmartParts();
			var uc = _outlookPanelContentWorker.GetOutlookPanelContent(_portalSettings.LastModule);

			if (uc == null)
				return;

			outlookBarWorkSpace1.SetNavigatorControl(uc, modulePanelItem.PreviewText, modulePanelItem.PreviewUrl);

			var navigator = uc as AbstractNavigator;
			if (navigator != null)
			{
				navigator.RefreshNavigator();
			}

			if (uc is ForecasterNavigator)
				((ForecasterNavigator)uc).SetMainOwner(this);

			if (uc is ForecasterNavigator || uc is PayrollExportNavigator)
			{
				webControl1.Visible = false;
				wfmWebControl.Visible = false;
				return;
			}

			wfmWebControl.Visible = false;
			webControl1.Visible = true;


			if (uc is SchedulerNavigator || uc is PeopleNavigator)
				((INavigationPanel)uc).SetMainOwner(this);

			if (uc is ShiftsNavigationPanel)
				((ShiftsNavigationPanel)uc).SetMainOwner(this);
		}

		private void webView1LoadFailed(object sender, LoadFailedEventArgs e)
		{
			showCustomerWebMenu = false;
			webControl1.Visible = false;
			//we can't goto static page in this event, for some reason
		}

		private void webView1BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			if (!showCustomerWebMenu)
			{
				e.Menu.Items.Clear();
				return;
			}

			foreach (var item in e.Menu.Items.Where(item => item.CommandId == CommandIds.ViewSource))
			{
				e.Menu.Items.Remove(item);
				break;
			}
			var menuItem = new EO.WebBrowser.MenuItem(UserTexts.Resources.StartPage, homeCommand);

			e.Menu.Items.Add(menuItem);
		}

		private void toolStripButtonCustomerWebClick(object sender, EventArgs e)
		{
			try
			{
				goToPublicPage(true);
			}
			catch (ArgumentException exception)
			{
				MessageDialogs.ShowError(this, exception.Message, UserTexts.Resources.ErrorMessage);
			}
			catch (Exception exception)
			{
				using (var view = new SimpleExceptionHandlerView(exception, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.CouldNotReachTeleoptiCustomerWebAtTheMoment))
				{
					view.ShowDialog();
				}
			}
		}

		private void outlookBar1SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
		{
			outlookBarWorkSpace1.SetHeader(e.SelectedItem);
			ModuleSelected(e.SelectedItem);
		}

		private void webView1NewWindow(object sender, NewWindowEventArgs e)
		{
			webView1.LoadUrl(e.TargetUrl);
			e.Accepted = false;
		}

		private void smartClientShellFormShown(object sender, EventArgs e)
		{
			Enabled = true;
			Cursor = Cursors.Default;

			if (!string.IsNullOrEmpty(_portalSettings.LastModule))
			{
				startModule(_portalSettings.LastModule);
			}
			else
			{
				startFirstEnabledModule();
			}

			toolStripStatusLabelSpring.Text = LanguageResourceHelper.Translate("XXReady");

			TopMost = true;
			Focus();
			BringToFront();
			TopMost = false;
		}

		private void webView1Command(object sender, CommandEventArgs e)
		{
			if (e.CommandId == homeCommand)
				goToPublicPage(true);
		}

		private void goToLocalPage()
		{
			webControl1.Visible = true;
			var executingAssembly = Assembly.GetExecutingAssembly();
			var pageName = executingAssembly.GetManifestResourceNames().First(n => n.Contains("EmptyStatic"));

			webView1.LoadHtml(GetFromResources(pageName));
		}

		public string GetFromResources(string resourceName)
		{
			Assembly assem = GetType().Assembly;
			using (Stream stream = assem.GetManifestResourceStream(resourceName))
			{
				if (stream == null) return "";
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private void goToPublicPage(bool gotoStart)
		{
			wfmWebControl.Visible = false;
			webControl1.Visible = true;
			if (!checkInternetConnection() || !_toggleManager.IsEnabled(Toggles.WFM_Connect_NewLandingPage_Remove_GDPR_78132))
			{
				showCustomerWebMenu = false;
				goToLocalPage();
				return;
			}
			showCustomerWebMenu = true;
			if (!gotoStart && webView1.Url != "") return;

			gotoCustomerWebAndLogOn();
		}

		private bool checkInternetConnection()
		{
			try
			{
				using (var client = new WebClient())
				{
					using (client.OpenRead("http://www.teleopti.com"))
					{
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
		}

		private async void gotoCustomerWebAndLogOn()
		{
			try
			{
				await Task.Run(() =>
				{
					var token = SingleSignOnHelper.SingleSignOn(LoggedOnPerson);
					webView1.Url = string.Format("http://www.teleopti.com/elogin.aspx?{0}", token);
					showCustomerWebMenu = true;
				});
			}
			catch (Exception exception)
			{
				_logger.Error("Can't access teleopti.com on startpage " + exception.Message);
				showCustomerWebMenu = false;
				goToLocalPage();
			}
		}

		private IPerson LoggedOnPerson
		{
			get
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return new PersonRepository(new ThisUnitOfWork(unitOfWork))
						.Get(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.PersonId);
				}
			}
		}

		private void openOptionsDialog()
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager, _container.Resolve<IBusinessRuleConfigProvider>(), _container.Resolve<IConfigReader>())));
				settings.Show();
				settings.BringToFront();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void handlingCertificateErrorsWebView1(object sender, CertificateErrorEventArgs e)
		{
			handleCertificateError(e, () =>
			{
				webView1.LoadHtml($"<!doctype html><html><head></head><body>The following url is missing a certificate. <br/> {e.Url} </body></html>");
				_logger.Error("The following url is missing a certificate. " + e.Url);
			});
		}


		private void handleCertificateError(CertificateErrorEventArgs e, Action errorCallback)
		{
			if ((int)e.ErrorCode == -214)
			{
				_logger.Info($"Certificate error ({e.ErrorCode}) for {e.Url}.");
				e.Continue();
			}
			else
			{
				errorCallback();
			}
		}

		private void handlingEoRuntimeErrors(object sender, ExceptionEventArgs e)
		{
			_logger.Error("Error in the EO browser", e.ErrorException);
		}

		private Uri buildWfmUri(string relativePath)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			return new Uri($"{wfmPath}{relativePath}");
		}
	}
}
