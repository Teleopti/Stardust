using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.SmartClientPortal.Shell.Controls;
using Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces;
using log4net;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.Permissions;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	/// <summary>
    /// Main application SmartClientShell view.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class SmartClientShellForm : BaseRibbonForm, IDummyInterface
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (SmartClientShellForm));
        private readonly IComponentContext _container;
        
        private readonly SystemCheckerValidator _systemChecker;
        private bool _lastSystemCheck = true;
        private readonly OutlookPanelContentWorker _outlookPanelContentWorker;
        private readonly PortalSettings _portalSettings;

        protected SmartClientShellForm()
        {
            using(PerformanceOutput.ForOperation("SmartClientPortal ctor"))
            {
                InitializeComponent();
				
	            if (!DesignMode)
	            {
		            SetTexts();
					
		            toolStripButtonSystemExit.Click += toolStripButtonSystemExit_Click;
		            toolStripButtonHelp.Click += toolStripButtonHelp_Click;
		            ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlAdv1BeforeContextMenuOpen;
	            }
            }
			KeyPreview = true;
			KeyDown += Form_KeyDown;
			KeyPress += Form_KeyPress;
		}

		void Form_KeyDown(object sender, KeyEventArgs e)
		{
			// key shortcuts
			if (e.Modifiers == Keys.Control)
			{
				if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
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
        }

        void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            ViewBase.ShowHelp(this,false);
        }

        private void toolStripButtonAbout_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog();
            if (about.LicenseWasApplied)
            {
                Close();
            }
        }

        private void toolStripButtonSystemOptions_Click(object sender, EventArgs e)
        {
	        var toggleManager = _container.Resolve<IToggleManager>();
            try
            {
					var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(toggleManager)));
                settings.Show();
                settings.BringToFront();
            }
		    catch (DataSourceException ex)
		    {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
            }
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
                StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode = true;
            }
			if (e.KeyCode == Keys.T && e.Shift && e.Alt)
			{
				StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = true;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void SmartClientShellForm_Load(object sender, EventArgs e)
		{
			var identity = (ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity;
			var loggedOnBu = identity.BusinessUnit;
			Text = UserTexts.Resources.TeleoptiRaptorColonMainNavigation + @" " + loggedOnBu.Name;
			ribbonControlAdv1.MenuButtonText = LanguageResourceHelper.Translate(ribbonControlAdv1.MenuButtonText);
			toolStripStatusLabelLicense.Text = toolStripStatusLabelLicense.Text + ApplicationTextHelper.LicensedToCustomerText;
			toolStripStatusLabelLoggedOnUser.Text = toolStripStatusLabelLoggedOnUser.Text +
			                                        ApplicationTextHelper.LoggedOnUserText;

            setNotifyData(_systemChecker.IsOk());

            LoadOutLookBar();

            InitializeSmartPartInvoker();

            Roger65(string.Empty);
            SetPermissionOnToolStripButtonControls();

            if (!string.IsNullOrEmpty(_portalSettings.LastModule))
            {
	            startModule(_portalSettings.LastModule);
            }
            else
            {
	            startFirstEnabledModule();
            }
 
            var showMemConfig = ConfigurationManager.AppSettings.Get("ShowMem");
            bool showMemBool;
            if (bool.TryParse(showMemConfig, out showMemBool))
            {
                if (showMemBool)
                {
                    showMem();
                }                
            }

			//var uri = new Uri("http://www.teleopti.com/wfmv8/landingpage/iframe");
			//webBrowser1.Navigate(uri);
			//webBrowser1.Visible = true;
		
		
		}

        private void showMem()
        {
	        toolStripStatusLabelRoger65.Visible = true;
            var t = new Timer { Interval = 1000, Enabled = true };
            t.Tick += updateMem;
        }

	    private long maxMem = 0;

	    private void updateMem(object sender, EventArgs e)
        {
	        var mem = GC.GetTotalMemory(true);
	        if (mem > maxMem)
		        maxMem = mem;
            Roger65(string.Format(CultureInfo.CurrentCulture, "Mem: {0:#.00} MB (max mem: {1:#} MB)", (double)mem/1024/1024, maxMem/1024/1024));
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
            if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage))
            {
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
                    using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
                    {
                        view.ShowDialog(this);
                    }
                    if(permissionForm != null)
                    {
                        permissionForm.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Saved event of the Permission Explorer form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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

        private void toolStripStatusLabelSpring_Click(object sender, EventArgs e)
        {
#if(DEBUG)
            string outPut = string.Concat(UnitOfWorkFactory.Current.NumberOfLiveUnitOfWorks.ToString(), " Raptor UOW:s");
            IUnitOfWorkFactory matrix = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.Statistic;
            if(matrix!=null)
                outPut +=string.Concat(", ", matrix.NumberOfLiveUnitOfWorks, " Matrix UOW:s");
            Roger65(outPut);
#endif
        }

        /// <summary>
        /// Handles the WorkspaceGridSizeChanged event of the gridWorkspace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-29
        /// </remarks>
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

        /// <summary>
        /// Sets the permissions on the form's toolstrip button controls.
        /// THIS IS THE BADING BALL!!!
        /// </summary>
        private void SetPermissionOnToolStripButtonControls()
        {
            var authorization = PrincipalAuthorization.Instance();
            toolStripButtonPermissions.Enabled =
                authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage);
            toolStripButtonSystemOptions.Enabled =
                authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
        	toolStripButtonMyProfile.Enabled =
                ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.AuthenticationSettings.LogOnMode !=
        		LogOnModeOption.Win &&
        		((IUnsafePerson) TeleoptiPrincipal.Current).Person.ApplicationAuthenticationInfo != null;
        }

        private void LoadOutLookBar()
        {
            var authorization = PrincipalAuthorization.Instance();
            IEnumerable<IApplicationFunction> modules = authorization.GrantedFunctionsBySpecification(new ModuleSpecification());

	        IList<ModulePanelItem> modulePanelItems = new List<ModulePanelItem>();
            foreach (IApplicationFunction module in modules.OrderBy(m => m.SortOrder.GetValueOrDefault(1000000)))
            {
                if(module.IsPreliminary)
                    continue;

                OutlookBarInfo outlookBarSmartPartInfo = new OutlookBarInfo();
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
                        outlookBarSmartPartInfo.Icon = Resources.WFM_People_32_32 ;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenForecasterPage:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Forecasts_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenSchedulePage:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Schedules_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenIntradayPage:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Intraday_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenPermissionPage:
                        outlookBarSmartPartInfo.Icon = Resources.ccc_permission;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.Shifts:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Shifts_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.AccessToReports:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Reports_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenOptionsPage:
                        outlookBarSmartPartInfo.Icon = Resources.ccc_Settings;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.OpenBudgets:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Budgets_old_32_32;
                        break;
                    case DefinedRaptorApplicationFunctionPaths.PayrollIntegration:
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Payroll_Integration_32_32;
                        break;
                    default:
                        // add default image as a resource to your module.
                        outlookBarSmartPartInfo.Icon = Resources.WFM_Performance_Manager_32_32;
                        break;
                }
				modulePanelItems.Add(new ModulePanelItem
				{
					ItemImage = outlookBarSmartPartInfo.Icon,
					ItemText = outlookBarSmartPartInfo.Title,
					ItemEnabled = outlookBarSmartPartInfo.Enable,
					Tag = outlookBarSmartPartInfo.EventTopicName
				});
            }

			outlookBar1.AddItems(modulePanelItems.ToArray());
        }

        /// <summary>
        /// Initializes the smart part invoker.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        private void InitializeSmartPartInvoker()
        {
            string smartPartPath = ConfigurationManager.AppSettings["smartPartPath"];

            var smartPartWorker = new SmartPartWorker(smartPartPath, gridWorkspace);
            var smartPartCommand = new SmartPartCommand(smartPartWorker);
            SmartPartInvoker.SmartPartCommand = smartPartCommand;
            SmartPartEnvironment.MessageBroker = StateHolder.Instance.StateReader.ApplicationScopeData.Messaging;
            SmartPartEnvironment.SmartPartWorkspace = gridWorkspace;
        }

        /// <summary>
        /// Sets the smart parts.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="smartPartId">The smart part id.</param>
        /// <param name="gridColumn">The grid column.</param>
        /// <param name="gridRow">The grid row.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 9/10/2008
        /// </remarks>
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
        }

        private void showBalloon()
        {
            notifyIcon.ShowBalloonTip(100);
        }

        private void setNotifyData(bool isOk)
        {
            if (isOk)
            {
                notifyIcon.Icon =  Resources.WFMIcon;
                notifyIcon.Text = UserTexts.Resources.CheckSystemOk;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipTitle = UserTexts.Resources.CheckSystemOk;
                notifyIcon.BalloonTipText = UserTexts.Resources.CheckSystemOk;
            }
            else
            {
                notifyIcon.Icon = Resources.WFMIcon;
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
                    var settings = new OptionDialog(new OptionCore(new MyProfileSettingPagesProvider()));
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

			outlookBarWorkSpace1.SetNavigatorControl(uc);
			
			var navigator = uc as AbstractNavigator;
			if (navigator != null)
			{
				navigator.RefreshNavigator();
			}

		    //var shifts = uc as Win.Shifts.ShiftsNavigationPanel;
		    //if (shifts != null && !startup)
		    //	shifts.OpenShifts();
	    }

        private void toolStripButtonCustomerWeb_Click(object sender, EventArgs e)
        {
            try
            {
                var token = SingleSignOnHelper.SingleSignOn();
                webBrowser1.Navigate(StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["CustomerWebSSOUrl"], "_blank", token, "Content-Type: application/x-www-form-urlencoded");
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

		private void outlookBar1_SizeChanged(object sender, EventArgs e)
		{
			splitContainer.Size = new Size(outlookBar1.Width, outlookBar1.Top);
		}

		private void outlookBar1_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
		{
			outlookBarWorkSpace1.SetHeader(e.SelectedItem);
			ModuleSelected(e.SelectedItem);
		}

       

    }
}
