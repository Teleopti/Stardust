using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using EO.Base;
using EO.WebBrowser;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Controls;
using log4net;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.SmartClientPortal.Shell.Win;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Backlog;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.OutlookControls.Workspaces;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Permissions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;
using Timer = System.Windows.Forms.Timer;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	/// <summary>
	/// Main application SmartClientShell view.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class SmartClientShellForm : BaseRibbonForm, IDummyInterface
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof (SmartClientShellForm));
		private readonly ILog _customLogger = LogManager.GetLogger("CustomEOLogger");
		private readonly IComponentContext _container;
		
		private readonly SystemCheckerValidator _systemChecker;
		private bool _lastSystemCheck = true;
		private readonly OutlookPanelContentWorker _outlookPanelContentWorker;
		private readonly PortalSettings _portalSettings;
		private readonly IToggleManager _toggleManager;
		readonly int homeCommand = CommandIds.RegisterUserCommand("StartPage");
		private bool showCustomerWebMenu = true;
		private const string _permissionModule = "/permissions";
		private WebUrlHolder _webUrlHolder;
		private List<string> validUrls;
		private bool showDataProtectionWebPage;

		protected SmartClientShellForm()
		{
			using(PerformanceOutput.ForOperation("SmartClientPortal ctor"))
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
			KeyDown += Form_KeyDown;
			KeyPress += Form_KeyPress;

			wfmWebView.RegisterJSExtensionFunction("errorStayingAlive",wfmWebView_JSerrorStayingAlive);
			webViewDataProtection.RegisterJSExtensionFunction("yesResponseCallback", yesResponse);
			webViewDataProtection.RegisterJSExtensionFunction("noOrNotNowResponseCallback", noResponse);
			EO.Base.Runtime.Exception += handlingEoRuntimeErrors;
			var enableLargeAddressSpaceSetting = ConfigurationManager.AppSettings["EOEnableLargeAddressSpace"];
			var enableLargeAddressSpace = false;
			if(bool.TryParse(enableLargeAddressSpaceSetting, out enableLargeAddressSpace))
				EO.Base.Runtime.EnableLargeAddressSpace = enableLargeAddressSpace;
		}

		private void yesResponse(object sender, JSExtInvokeArgs jsExtInvokeArgs)
		{
			logInfo(" Yes was clicked on DataProtection EO:URL= " + webViewDataProtection.Url);
			showDataProtectionWebPage = false;
			webControlDataProtection.Visible = false;
			webControl1.Visible = true;
			gotoCustomerWebAndLogOn();
		}

		private void noResponse(object sender, JSExtInvokeArgs jsExtInvokeArgs)
		{
			logInfo(" No was clicked on DataProtection EO:URL= " + webViewDataProtection.Url);
			showDataProtectionWebPage = false;
			webControlDataProtection.Visible = false;
			webControl1.Visible = true;
			gotoCustomerWeb();
		}

		private void logInfo(string message)
		{
			_customLogger.Info("SmartClientPortal: EoBrowser: " + message);
		}

		private void wfmWebView_JSerrorStayingAlive(object sender, JSExtInvokeArgs e)
		{
			notifyIcon.Icon = Resources.NotifyWarning;
			notifyIcon.Text = UserTexts.Resources.CheckSystemWarning;
			notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
			notifyIcon.BalloonTipTitle = UserTexts.Resources.CheckSystemWarning;
			notifyIcon.BalloonTipText = "The web channel is disconnected." +
										Environment.NewLine + 
										"You may require to relogon to use permissions.";
			showBalloon();
			logInfo("Session dropped in StayingAlive EO:URL " + wfmWebView.Url);
			wfmWebView.LoadUrl(webServer + "start/Url/RedirectToWebLogin");
		}		

		private void setBusinessUnitInWfmWebView()
		{
			if (!wfmWebControl.Enabled) return;
			var bu = ((ITeleoptiIdentity) TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit.Id;
			var request = new Request(webServer + "Start/AuthenticationApi/Logon");
			request.PostData.AddValue("businessUnitId", bu.GetValueOrDefault().ToString());
			request.Method = "post";
			wfmWebView.LoadCompleted += wfmWebViewOnLoadCompletedSetBusinessUnit;
			logInfo("setBusinessUnitInWfmWebView: Setting the business unit");
			wfmWebView.LoadRequest(request);
		}

		private void wfmWebViewOnLoadCompletedSetBusinessUnit(object sender, LoadCompletedEventArgs loadCompletedEventArgs)
		{
			wfmWebView.LoadCompleted -= wfmWebViewOnLoadCompletedSetBusinessUnit;
			if (_toggleManager.IsEnabled(Toggles.WfmPermission_ReplaceOldPermission_34671))
			{
				logInfo(" Setting the permissions url current URL is " + wfmWebView.Url );
				setWfmWebUrl();
			}
		}

		private void setBusinessUnitInDataProtectionWebView()
		{		
			var bu = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit.Id;
			var request = new Request(webServer + "Start/AuthenticationApi/Logon");
			request.PostData.AddValue("businessUnitId", bu.GetValueOrDefault().ToString());
			request.Method = "post";
			webViewDataProtection.LoadCompleted += dataProtectionWebViewOnLoadCompletedSetBusinessUnit;
			logInfo("setBusinessUnitInDataProtectionWebView: Setting the business unit");
			webViewDataProtection.LoadRequest(request);
		}

		private void dataProtectionWebViewOnLoadCompletedSetBusinessUnit(object sender, LoadCompletedEventArgs loadCompletedEventArgs)
		{
			logInfo(" Bu loaded for data protection EO:URL= " + webViewDataProtection.Url);
			webViewDataProtection.LoadCompleted -= dataProtectionWebViewOnLoadCompletedSetBusinessUnit;
			webControlDataProtection.WebView.Url = string.Format("{0}WFM/index_desktop_client.html#/fdpa", webServer);
		}

		private int cnt;
		private void keepWfmAlive()
		{
			if (!wfmWebControl.Enabled) return;
			cnt ++;
			if (cnt < 300) return;
			cnt = 0;
			// to see if this will help on bug 39438
			if (!wfmWebView.IsCreated)
			{
				logInfo("keepWfmAlive: wfmWebView is not created");
				return;
			}
			// but maybe we will get a login instead of permissions after some time instead
			JSObject window = wfmWebView.GetDOMWindow();
			if (wfmWebView.CanEvalScript)
			{
				try
				{
					logInfo("EvalScript(ahAhAhAhStayingAlive) EO:URL =" + wfmWebView.Url);
					var ahAhAhAhStayingAlive = (JSFunction)wfmWebView.EvalScript("ahAhAhAhStayingAlive");
					if (ahAhAhAhStayingAlive != null)
					{
						logInfo("Invoke StayingAlive EO:URL =" + wfmWebView.Url);
						ahAhAhAhStayingAlive.Invoke(window, new object[] { });
					}
				}
				catch (JSInvokeException)
				{
					logInfo("JSInvokeException");
				}
			}
		}

		void Form_KeyDown(object sender, KeyEventArgs e)
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
			_webUrlHolder = _container.Resolve<WebUrlHolder>();
			
			if (!_toggleManager.IsEnabled(Toggles.WfmPermission_ReplaceOldPermission_34671))
			{
				wfmWebControl.Enabled = false;
				wfmWebControl.Visible = false;
			}
			validUrls = new List<string>()
			{
				"WFM/index_desktop_client.html#" + _permissionModule,
				"start/Url/RedirectToWebLogin",
				"SSO/",
				"Authentication",
				"/response?dnoa.userSuppliedIdentifier",
				"/Start/Return/HandleReturn",
				"sample-with-policyengine",
				"authenticate?whr=urn:Windows",
				"/OpenId/Provider?openid",
				"/OpenId/TriggerWindowsAuthorization"
			};

			setBusinessUnitInWfmWebView();

			wfmWebView.BeforeContextMenu += wfmWebView_BeforeContextMenu;
		}

		void toolStripButtonHelp_Click(object sender, EventArgs e)
		{
			ViewBase.ShowHelp(this,false);
		}

		private void toolStripButtonAbout_Click(object sender, EventArgs e)
		{
			var about = new About();
			about.ShowDialog();
			if (about.ProductActivationKeyWasApplied)
			{
				Close();
			}
		}

		private void toolStripButtonSystemOptions_Click(object sender, EventArgs e)
		{
			openOptionsDialog();
		}

		private void toolStripButtonSystemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		public StatusStrip MainStatusStrip
		{
			get { return _mainStatusStrip; }
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
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
		private void SmartClientShellForm_Load(object sender, EventArgs e)
		{
			Enabled = false;
			Cursor = Cursors.WaitCursor;
			toolStripStatusLabelSpring.Text = LanguageResourceHelper.Translate("XXLoadingThreeDots");
			_mainStatusStrip.Refresh();
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			var loggedOnBu = identity.BusinessUnit;
			Text = UserTexts.Resources.TeleoptiRaptorColonMainNavigation + @" " + loggedOnBu.Name;
			
			toolStripStatusLabelLicense.Text = toolStripStatusLabelLicense.Text + ApplicationTextHelper.LicensedToCustomerText;
			toolStripStatusLabelLoggedOnUser.Text = toolStripStatusLabelLoggedOnUser.Text +
													ApplicationTextHelper.LoggedOnUserText;
			goToPublicPage(false);

			setNotifyData(_systemChecker.IsOk());

			LoadOutLookBar();

			InitializeSmartPartInvoker();

			Roger65(string.Empty);
			SetPermissionOnToolStripButtonControls();

			var showMemConfig = ConfigurationManager.AppSettings.Get("ShowMem");
			bool showMemBool;
			if (bool.TryParse(showMemConfig, out showMemBool))
			{
				if (showMemBool)
				{
					showMem();
				}
			}

			backStage1.Controls.Remove(backStageButtonSignCustomerWeb);

			_container.Resolve<HangfireClientStarter>().Start();
		}

		public void setWfmWebUrl()
		{
			logInfo(" URL before setting the permissions EO:URL " + wfmWebView.Url);
			if (!wfmWebView.IsCreated)
			{
				logInfo(" wfmWebView is not created EO:URL " + wfmWebView.Url);
				wfmWebView = new WebView();
				wfmWebView.UrlChanged += wfmWebView_UrlChanged;
				wfmWebControl.WebView = wfmWebView;
			}
			wfmWebView.LoadUrl(string.Format("{0}WFM/index_desktop_client.html#{1}", webServer, _permissionModule));
			logInfo(" Loaded permissions via load method EO:URL " + wfmWebView.Url);
		}

		private string webServer
		{
			get
			{
				return _webUrlHolder.WebUrl;
			}
		}

		private void showMem()
		{
			toolStripStatusLabelRoger65.Visible = true;
			var t = new Timer { Interval = 1000, Enabled = true };
			t.Tick += updateMem;
		}
		
		private void updateMem(object sender, EventArgs e)
		{
			Roger65(MemoryCounter.DefaultInstance().CurrentMemoryConsumptionString());
		}

		private void SmartClientShellForm_FormClosing(object sender, FormClosingEventArgs e)
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
				_logger.Error("An error occurred when trying to save settings on exit.",dataSourceException);
			}
			
			StateHolder.Instance.Terminate();
		}

		private void toolStripButtonPermissons_Click(object sender, EventArgs e)
		{
			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage))
			{
				if (!_toggleManager.IsEnabled(Toggles.WfmPermission_ReplaceOldPermission_34671))
				{
					toggleWebControls(true);
					PermissionsExplorer permissionForm = null;
					try
					{
						permissionForm = new PermissionsExplorer(_container);
						permissionForm.LoadDatabaseData();
						permissionForm.Saved += permissionForm_Saved;
						permissionForm.Show();
					}
					catch (DataSourceException dataSourceException)
					{
						using (
							var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC,
								UserTexts.Resources.ServerUnavailable))
						{
							view.ShowDialog(this);
						}
						if (permissionForm != null)
						{
							permissionForm.Close();
						}
					}
				}
				else
				{
					logInfo("toolStripButtonPermissons_Click: Showing permissions EO:URL " + wfmWebView.Url);
					backStageViewMain.HideBackStage();
					toggleWebControls(false);
				}
			}
		}

		private void permissionForm_Saved(object sender, EventArgs e)
		{
			SetPermissionOnToolStripButtonControls();
		   // SetPermissionOnOutlookBarSmartButtonControls();
		}

		private void persistSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonalSettingDataRepository(uow).PersistSettingValue(_portalSettings);
				uow.PersistAll();
			}
		}

		private void Roger65(string message)
		{
			toolStripStatusLabelRoger65.Text = message;
			toolStripStatusLabelRoger65.ForeColor = Color.Red;
		}
		
		private void GridWorkspace_WorkspaceGridSizeChanged(object sender, EventArgs e)
		{
			gridWorkspace.RemoveAllSmartPart();
			if (gridWorkspace.GridSize == GridSizeType.TwoByOne)
			{
				//Show Default smart parts
				string url1 = ConfigurationManager.AppSettings.Get("SmartPartUrl1");
				string url2 = ConfigurationManager.AppSettings.Get("SmartPartUrl2");
				SetSmartParts(url1, 1, 0, 0);
				SetSmartParts(url2, 2, 0, 1);
			}
		}

		private void SetPermissionOnToolStripButtonControls()
		{
			var authorization = PrincipalAuthorization.Current();
			backStageButtonPermissions.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage);
			backStageButtonOptions.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
			var type = LogonPresenter.AuthenticationTypeOption;
			backStageButtonMyProfile.Enabled = type.Equals(AuthenticationTypeOption.Application);
		}

		private void LoadOutLookBar()
		{
			var authorization = PrincipalAuthorization.Current();
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
						if (_toggleManager.IsEnabled(Toggles.Wfm_People_PrepareForRelease_39040) &&
							PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPeople))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewPeopleModule;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/people");
						}
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenForecasterPage:
						outlookBarSmartPartInfo.Icon = Resources.Forecasts2_filled_32x32;
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenSchedulePage:
						outlookBarSmartPartInfo.Icon = Resources.Schedules_filled_space_32x32;
						if (_toggleManager.IsEnabled(Toggles.Wfm_Requests_PrepareForRelease_38771) &&
							PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebRequests))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewRequestsModule;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/requests");
						}
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenIntradayPage:
						outlookBarSmartPartInfo.Icon = Resources.Intraday_filled_space_32x32;
						if (!_toggleManager.IsEnabled(Toggles.Wfm_Web_Intraday_Rta_As_first_Choice_42206) &&
							_toggleManager.IsEnabled(Toggles.Wfm_Intraday_38074) && 
							PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebIntraday))
						{
							outlookBarSmartPartInfo.PreviewText = UserTexts.Resources.PreviewTheNewIntradayTool;
							outlookBarSmartPartInfo.PreviewUrl = buildWfmUri("WFM/#/intraday");
						}
						break;
					case DefinedRaptorApplicationFunctionPaths.OpenPermissionPage:
						outlookBarSmartPartInfo.Icon = Resources.WFM_Teleopti_WFM_main_small;
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

			if (_toggleManager.IsEnabled(Toggles.Backlog_Module_23980))
				modulePanelItems.Add(new ModulePanelItem
				{
					ItemImage = Resources.help_32,
					ItemText = "Backlog",
					ItemEnabled = true,
					Tag = "Raptor/PersonAdmin"
				});

			outlookBar1.AddItems(modulePanelItems.ToArray());
		}

		private void InitializeSmartPartInvoker()
		{
			string smartPartPath = ConfigurationManager.AppSettings["smartPartPath"];

			var smartPartWorker = new SmartPartWorker(smartPartPath, gridWorkspace);
			var smartPartCommand = new SmartPartCommand(smartPartWorker);
			SmartPartInvoker.SmartPartCommand = smartPartCommand;
			SmartPartEnvironment.MessageBroker = MessageBrokerInStateHolder.Instance;
			SmartPartEnvironment.SmartPartWorkspace = gridWorkspace;
		}

		private void SetSmartParts(string url, int smartPartId,int gridColumn, int gridRow)
		{
			var smartPartInfo = new SmartPartInformation
									{
										ContainingAssembly = "Teleopti.Common.UI.SmartPartControls.RaptorSmartParts",
										SmartPartName =
											"Teleopti.Common.UI.SmartPartControls.RaptorSmartParts.WebSmartPart",
										SmartPartHeaderTitle = "Web Smart Part",
										GridColumn = gridColumn,
										GridRow = gridRow,
										SmartPartId = smartPartId.ToString(CultureInfo.CurrentCulture)
									};

			// Create SmartPart Parameters  [optional]
			IList<SmartPartParameter> parameters = new List<SmartPartParameter>();
			var parameter = new SmartPartParameter("Url", url);
			parameters.Add(parameter);

			try
			{
				// Invoke SmartPart
				SmartPartInvoker.ShowSmartPart(smartPartInfo, parameters);
			}
			catch (FileLoadException fileLoadException)
			{
				_logger.Error("File load exception when loading smart part.", fileLoadException);
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				_logger.Error("File not found exception when loading smart part.", fileNotFoundException);
			}
		}

		private void notifyTimer_Tick(object sender, EventArgs e)
		{
			bool isOk = _systemChecker.IsOk();
			if (_lastSystemCheck != isOk)
			{
				setNotifyData(isOk);
				showBalloon();
				_lastSystemCheck = isOk;
			}
			keepWfmAlive();
		}

		private void showBalloon()
		{
			notifyIcon.ShowBalloonTip(100);
		}

		private void setNotifyData(bool isOk)
		{
			if (isOk)
			{
				notifyIcon.Icon =  Resources.NotifyOK;
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

		private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
		{
			showBalloon();
		}

		private void toolStripButtonMyProfile_Click(object sender, EventArgs e)
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

			if (modulePanelItem.ItemText == "Backlog" && _toggleManager.IsEnabled(Toggles.Backlog_Module_23980))
			{
				//using (var backLogSelector = new BacklogSelector(_container))
				//{
				//	backLogSelector.ShowDialog(this);
				//}

				var outboundView = new OutboundView(_container);
				outboundView.Show(this);				
			}

			_portalSettings.LastModule = modulePanelItem.Tag.ToString();
			SmartPartInvoker.ClearAllSmartParts();
			var uc = _outlookPanelContentWorker.GetOutlookPanelContent(_portalSettings.LastModule, _toggleManager);

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
				webControlDataProtection.Visible = false;
				return;
			}

			if (showDataProtectionWebPage)
			{
				wfmWebControl.Visible = false;
				webControl1.Visible = false;
				webControlDataProtection.Visible = true;
			}
			else
			{
				wfmWebControl.Visible = false;
				webControlDataProtection.Visible = false;
				webControl1.Visible = true;
			}
				

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

		private void webView1_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
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
			var menuItem = new EO.WebBrowser.MenuItem(UserTexts.Resources.StartPage,homeCommand);
			
			e.Menu.Items.Add(menuItem);
		}

		private void webViewDataProtection_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			e.Menu.Items.Clear();
		}

		private void toolStripButtonCustomerWeb_Click(object sender, EventArgs e)
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

		private void outlookBar1_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
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

		private void webView1_Command(object sender, CommandEventArgs e)
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
			webControlDataProtection.Visible = false;
			webControl1.Visible = true;
			if (!checkInternetConnection())
			{
				showCustomerWebMenu = false;
				goToLocalPage();
				return;
			}
			showCustomerWebMenu = true;
			if (!gotoStart && webView1.Url != "") return;
			if (string.IsNullOrWhiteSpace(LoggedOnPerson.Email))
			{
				pleaseRegisterAnEmailAddress(UserTexts.Resources.PleaseConfigureYourEmailAddress);
				return;
			}
			if (!_toggleManager.IsEnabled(Toggles.Landing_Page_Data_Protection_Question_35721))
			{
				gotoCustomerWebAndLogOn();
				return;
			}

			var dataProtectionResponse = dataProtectionSetting();
			switch (dataProtectionResponse)
			{
				case DataProtectionEnum.Yes:
					gotoCustomerWebAndLogOn();
					break;
				case DataProtectionEnum.No:
					gotoCustomerWeb();
					break;
				default:
					showDataProtectionWebPage = true;
					wfmWebControl.Visible = false;
					webControl1.Visible = false;
					webControlDataProtection.Visible = true;
					setBusinessUnitInDataProtectionWebView();
					break;
			}
		}

		private bool checkInternetConnection()
		{
			try
			{
				using (var client = new WebClient())
				{
					using (var stream = client.OpenRead("http://www.teleopti.com"))
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

		private void pleaseRegisterAnEmailAddress(string message)
		{
			const string html = "<!doctype html><html><head></head><body style='font-family:Sans-Serif'>{0}</body></html>";
			webView1.LoadHtml(string.Format(html, message));
			showCustomerWebMenu = true;
		}

		private async void gotoCustomerWeb()
		{
			await Task.Run(() =>
			{
				webView1.Url = "http://www.teleopti.com/elogin.aspx?";
				showCustomerWebMenu = true;
			});
			
		}

		private DataProtectionEnum dataProtectionSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ISettingDataRepository settingDataRepository = new PersonalSettingDataRepository(uow);
				var dataProtectionResponse = settingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
				return dataProtectionResponse.Response;
			}
		}

		private IPerson LoggedOnPerson
		{
			get
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(unitOfWork)));
				}
			}
		}

		private void openOptionsDialog()
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager, _container.Resolve<IBusinessRuleConfigProvider>())));
				settings.Show();
				settings.BringToFront();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void toggleWebControls(bool hidePermissions)
		{
			wfmWebControl.Visible = !hidePermissions;
			logInfo("Toggling the wfm view EO:URL" + wfmWebView.Url);
			if (!hidePermissions)
			{
				webControl1.Visible = false;
				webControlDataProtection.Visible = false;
			}
			else
			{
				if (showDataProtectionWebPage)
				{
					webControlDataProtection.Visible = true;
					webControl1.Visible = false;
				}

				else
				{
					webControlDataProtection.Visible = false;
					webControl1.Visible = true;
				}
			}		
		}

		private void wfmWebView_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			e.Menu.Items.Clear();
		}

		private void handlingCertificateErrorsWfmWebView(object sender, CertificateErrorEventArgs e)
		{
			wfmWebView.LoadCompleted -= wfmWebViewOnLoadCompletedSetBusinessUnit;
			wfmWebView.LoadHtml($"<!doctype html><html><head></head><body>The following url is missing a certificate. <br/> {e.Url} </body></html>");
			_customLogger.Error("The following url is missing a certificate. " + e.Url + "  EO:URL " + wfmWebView.Url);
			_logger.Error("The following url is missing a certificate. " + e.Url);
		}

		private void handlingCertificateErrorsWebView1(object sender, CertificateErrorEventArgs e)
		{
			webView1.LoadHtml($"<!doctype html><html><head></head><body>The following url is missing a certificate. <br/> {e.Url} </body></html>");
			_logger.Error("The following url is missing a certificate. " + e.Url);
		}

		private void handlingCertificateErrorswebViewDataProtection(object sender, CertificateErrorEventArgs e)
		{
			webViewDataProtection.LoadHtml($"<!doctype html><html><head></head><body>The following url is missing a certificate. <br/> {e.Url} </body></html>");
			_customLogger.Error("The following url is missing a certificate. " + e.Url + "  EO:URL " + wfmWebView.Url);
			_logger.Error("The following url is missing a certificate. " + e.Url);
		}

		private void handlingEoRuntimeErrors(object sender, ExceptionEventArgs e)
		{
			_logger.Error("Error in the EO browser", e.ErrorException);
		}
		
		private void handlingLoadFailedError(object sender, LoadFailedEventArgs e)
		{
			_logger.Error(e.ErrorMessage + "Url: " + e.Url);
			e.UseDefaultMessage();
		}

		private Uri buildWfmUri(string relativePath)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			return new Uri($"{wfmPath}{relativePath}");
		}

		private void wfmWebView_UrlChanged(object sender, EventArgs e)
		{
			if (!((wfmWebView.Url == "" || wfmWebView.Url == webServer) ||
				validUrls.Any(x => wfmWebView.Url.Contains(x))))
			{
				setWfmWebUrl();
			}
		}
	}
}
