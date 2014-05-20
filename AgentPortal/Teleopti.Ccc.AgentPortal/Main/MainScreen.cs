using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.ServiceModel;
using System.Threading;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Schedule;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView;
using Teleopti.Ccc.AgentPortal.AgentScheduleMessenger;
using Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Controls;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Requests.RequestMaster;
using Teleopti.Ccc.AgentPortal.Settings;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleReporting;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using ClipboardControl = Teleopti.Ccc.AgentPortal.AgentPreferenceView.ClipboardControl;
using DayOff = Teleopti.Ccc.AgentPortalCode.Common.DayOff;
using ScheduleControl = Teleopti.Ccc.AgentPortal.AgentSchedule.ScheduleControl;
using ShiftCategory=Teleopti.Ccc.AgentPortalCode.Common.ShiftCategory;
using ToolStripItemClickedEventArgs=
    Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs;

namespace Teleopti.Ccc.AgentPortal.Main
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MainScreen : BaseRibbonForm, IToggleButtonState
    {
        private static string _reportUrl;
        private static string _targetFrame;
        private const string matrixWebSiteUrl = "MatrixWebSiteUrl";
    	private ScheduleMessengerScreen _scheduleMessengerScreen;
        private MailButton _mailButton;
        private readonly PushMessageController _controller = PushMessageController.CreatePushMessageHelper();
        private ViewConstructor _viewConstructor;
        private ClipboardControl _clipboardControl;
        private ClipboardControl _clipboardControlStudentAvailability;
        private readonly string _matrixBaseUrl;
        private bool _lastExportButtonState = true;
        private object _currentTemplate;
        private bool _toolStripGalleryPreferencesLoaded;
        private ToolStripControlHost _clipboardHostStudent;

        internal static string SelectedReport
        {
            get { return _reportUrl; }
        }

        internal static string SelectedTargetFrame
        {
            get { return _targetFrame; }
        }

    	internal static bool IsMessengerLoaded { get; set; }

    	internal static string LicenseHolderName { get; set; }

        public Control PanelMain
        {
            get { return panelSchedule; }
        }

        public Control PanelRightFirst
        {
            get { return panelLegends; }
        }       

        public void RefreshTab()
        {
            if (ActiveControl != null)
            {
                if (ActiveControl.GetType() == typeof(ScheduleControl))
                {
                    ((ScheduleControl)ActiveControl).RefreshSchedule();
                }
            }
        }

        public MainScreen()
        {
            SuspendLayout();
            InitializeComponent();

            // bugfix to 16626" Error with Arabic culture and language ar-EG
            // at the same time the following line is removed from designer: 
            //  this.xpTaskBar1.Style = Syncfusion.Windows.Forms.Tools.XPTaskBarStyle.Office2007;
            if (RightToLeft == RightToLeft.No)
                xpTaskBar1.Style = XPTaskBarStyle.Office2007;
            //

            _matrixBaseUrl = StateHolder.Instance.StateReader.SessionScopeData.AppSettings[matrixWebSiteUrl];
            try
            {
                BuildReportsTooItems();
            }
            catch (WebException ex)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture,
                                                    Resources.ErrorOccuredWhenAccessingTheDataSource +
                                                    ".\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(errorMessage, Resources.AgentPortal);
            }

            SetupMailButton();
            InitializeMainScreen();
            AgentScheduleStateHolder.Instance().FillDaysOff();
            AgentScheduleStateHolder.Instance().FillAbsences();
            InitializeRibbonBar();

            if (!DesignMode) SetTexts();

            toolStripStatusLabelLicense.Text = String.Concat(Resources.LicensedToColon, " ", LicenseHolderName);
            toolStripExMessages.Text = Resources.Messages;

			Resize += MainScreen_Resize;

			RefreshTab();
            ResumeLayout();
        }

        static void ribbonControlBeforeContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
            e.Cancel = true;
        }

        private void SetupMailButton()
        {
            _mailButton = new MailButton();
            _mailButton.SubscribeToMessageEvent(_controller);

            var host = new ToolStripControlHost(_mailButton) {BackColor = Color.Transparent, Dock = DockStyle.Fill};
        	toolStripExMessages.Items.Add(host);
            toolStripExMessages.Visible = PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm);

        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm))
                {
                    LoadAsm();
                }
                _viewConstructor = ViewConstructor.Instance;
                _viewConstructor.ScheduleViewChanged += Instance_ScheduleViewChanged;
                toolStripExView.Size = toolStripExView.PreferredSize;
                toolStripExNavigate.Size = toolStripExNavigate.PreferredSize;
                toolStripEx1.Size = toolStripEx1.PreferredSize;
                toolStripExShow.Size = toolStripExShow.PreferredSize;
                toolStripExMessages.Size = toolStripExMessages.PreferredSize;
                ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlBeforeContextMenuOpen;
            }
        }

        private void MainScreen_Resize(object sender, EventArgs e)
        {
            if (PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm))
            {
                // Popup the ASM
                if (WindowState == FormWindowState.Minimized)
                {
                    switchToAsm();
                }
            }

            if (WindowState != FormWindowState.Minimized)
                checkPermissions();
        }

        private void switchToAsm()
        {
            _mailButton.CloseMessageForm();
            Hide();
            _scheduleMessengerScreen.Show();
            _scheduleMessengerScreen.WindowState = FormWindowState.Normal;
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Dispose the user session on the server
            SdkServiceHelper.LogOnServiceClient.LogOffUser();
            //Dispose the sdk service proxy
            SdkServiceHelper.Dispose();
			if (_scheduleMessengerScreen != null)
				_scheduleMessengerScreen.Dispose();
        	_scheduleMessengerScreen = null;
        }

        private void toolStripButtonSchedule_Click(object sender, EventArgs e)
        {
            HidePreferenceTab();
            HideStudentAvailabilityTab();
            ToggleButtonEnabled("toolStripButtonExport", _lastExportButtonState);
            ShowRequestTab(false);
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Schedule, PanelMain, this);

            ActiveControl = hosted;
            SetupHelpContext(hosted);
        }

        void Instance_ScheduleViewChanged(object sender, EventArgs e)
        {
            var control = panelSchedule.Controls[0] as ScheduleControl;
            if (control == null) return;
            ToggleButtonEnabled("toolStripButtonExport", false);
            if (control.ScheduleType == ScheduleViewType.Month || (control.ScheduleType == ScheduleViewType.CustomWeek && control.ScheduleView.LoadedPeriod.DayCount() > 6))
                ToggleButtonEnabled("toolStripButtonExport", true);
        }

        private void ToolStripMenuSomeReportClick(object sender, EventArgs e)
        {
            var clickedToolStripMenuItem = (ToolStripMenuItem)sender;
            bool succeed = MatrixAgent.TryParseToUrl(clickedToolStripMenuItem, out _reportUrl, out _targetFrame);
            if (succeed)
            {
                _reportUrl = _matrixBaseUrl + _reportUrl;
                Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Report, PanelMain, this);
                if (hosted != null)
                {
                    ActiveControl = hosted;
                    SetupHelpContext(hosted);
                    toolStripButtonExport.Enabled = false;
                }
            }
        }

        private void MyReportItemClick(object sender, EventArgs e)
        {
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.MyReport, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            toolStripButtonExport.Enabled = false;
        }

        private void toolStripButtonScoreCard_Click(object sender, EventArgs e)
        {
            HidePreferenceTab();
            HideStudentAvailabilityTab();
            ShowRequestTab(false);
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Scorecard, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            toolStripButtonExport.Enabled = false;
        }

        private void toolStripButtonRequests_Click(object sender, EventArgs e)
        {
            HidePreferenceTab();
            HideStudentAvailabilityTab();
            ShowRequestTab(false);
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Requests, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            ShowRequestTab(true);
            toolStripButtonExport.Enabled = false;
        }

        private void ShowRequestTab(bool visible)
        {
            toolStripTabItemRequests.Enabled = visible;
            toolStripTabItemRequests.Checked = visible;
            toolStripTabItemRequests.Visible = visible;
        }

        private void toolStripButtonASM_Click(object sender, EventArgs e)
        {
            if (PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm))
            {
                HidePreferenceTab();
                HideStudentAvailabilityTab();
                ShowRequestTab(false);
                switchToAsm();
            }
        }

        private void officeButtonExitAgentPortal_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void officeButtonAgentPortalOptions_Click(object sender, EventArgs e)
        {
            var dialog = new AgentPortalOptions {StartPosition = FormStartPosition.CenterScreen};
        	dialog.ShowDialog(this);
        }

        private void toolStripButtonShowRequests_Click(object sender, EventArgs e)
        {
            var control = panelSchedule.Controls[0] as ScheduleControl;
            if (control == null) return;
            var button = sender as ToolStripButton;

            if (button != null)
            {
                control.ScheduleView.
                    AddVisualizationFilter(ScheduleAppointmentTypes.Request, button.Checked);
            }
        }

        private void InitializeMainScreen()
        {
            Text = Resources.AgentPortal;
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Schedule, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted); //store this control's help context
            _clipboardControlStudentAvailability = new ClipboardControl { Name = "clipboardControlStudentAvailability" };
            _clipboardHostStudent = new ToolStripControlHost(_clipboardControlStudentAvailability);
            ViewConstructor.Instance.BuildPortalView(ViewType.Legend, PanelRightFirst, this);
            Text += " : " + StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.Name;

            teleoptiToolStripGalleryPreferences.CheckOnClick = false;
            StartMessageBrokerListener();
            checkPermissions();
        }

        private void checkPermissions()
        {
            var preferencePermitted = PermissionService.Instance().IsPermitted(
                    ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyShiftCategoryPreferences) ||
                PermissionService.Instance().IsPermitted(
                    ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyExtendedPreferences);
            var studentAvailabilityPermitted = PermissionService.Instance().IsPermitted(
                    ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateStudentAvailability);

            toolStripButtonPreference.Visible = preferencePermitted;
            toolStripButtonViewStudentAvailability.Visible = studentAvailabilityPermitted;
            toolStripExClipboard.Items.Clear();
            toolStripExSAClipboard.Items.Clear();
            if (preferencePermitted) SetupPreferenceViewClipboardControl();
            if (studentAvailabilityPermitted) setupStudentAvailabilityViewClipboardControl();
        }
        private void setupStudentAvailabilityViewClipboardControl()
        {
            toolStripExClipboard.Items.Add(_clipboardHostStudent);
            toolStripExSAClipboard.Items.Add(_clipboardHostStudent);
            _clipboardControlStudentAvailability.CopyClicked += _clipboardControlStudentAvailability_CopyClicked;
            _clipboardControlStudentAvailability.CutClicked += _clipboardControlStudentAvailability_CutClicked;
            _clipboardControlStudentAvailability.PasteClicked += _clipboardControlStudentAvailability_PasteClicked;
            _clipboardControlStudentAvailability.ToolStripSplitButtonCopy.Text = Resources.Copy;
            _clipboardControlStudentAvailability.ToolStripSplitButtonCut.Text = Resources.Cut;
            _clipboardControlStudentAvailability.ToolStripSplitButtonPaste.Text = Resources.Paste;
            _clipboardControlStudentAvailability.SetButtonState(ClipboardAction.Paste, false);
            _clipboardControlStudentAvailability.ToolStripSplitButtonPaste.Enabled = false; 
        }

        private void _clipboardControlStudentAvailability_PasteClicked(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView != null)
            {
                studentAvailabilityView.PasteClip();
            }
        }

        private void _clipboardControlStudentAvailability_CutClicked(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView != null)
            {
                studentAvailabilityView.CutClip();
            }
        }

        private void _clipboardControlStudentAvailability_CopyClicked(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView != null)
            {
                studentAvailabilityView.CopyToClipboard();
            }
        }

        private void SetupPreferenceViewClipboardControl()
        {
            _clipboardControl = new ClipboardControl {Name = "clipboardControl"};
            var clipboardhost = new ToolStripControlHost(_clipboardControl);
            toolStripExClipboard.Items.Add(clipboardhost);
            _clipboardControl.CopyClicked += _clipboardControl_CopyClicked;
            _clipboardControl.CutClicked += _clipboardControl_CutClicked;
            _clipboardControl.PasteClicked += _clipboardControl_PasteClicked;
            _clipboardControl.ToolStripSplitButtonCopy.Text = Resources.Copy;
            _clipboardControl.ToolStripSplitButtonCut.Text = Resources.Cut;
            _clipboardControl.ToolStripSplitButtonPaste.Text = Resources.Paste;
            _clipboardControl.SetButtonState(ClipboardAction.Paste, false);
            _clipboardControl.ToolStripSplitButtonPaste.Enabled = false;
        }

        void _clipboardControl_PasteClicked(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView != null)
            {
                preferenceView.PasteClip();
            }
        }

        void _clipboardControl_CutClicked(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView != null)
            {
                preferenceView.CutClip();
            }
        }

        void _clipboardControl_CopyClicked(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView != null)
            {
                preferenceView.CopyToClipboard();
            }
        }

        private void InitializeRibbonBar()
        {
            if (!PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenScorecard))
            {
                toolStripExView.Items.Remove(toolStripButtonScoreCard);
            }
            if (!PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm))
            {
                toolStripExView.Items.Remove(toolStripButtonASM);
            }

            toolStripTabItemHome.Panel.Size = toolStripTabItemHome.Panel.PreferredSize;

            if (!PermissionService.Instance().ToCreateAnyRequest)
            {
                toolStripExView.Items.Remove(toolStripButtonRequests);
                toolStripExShow.Items.Remove(toolStripButtonShowRequests);
            }

            foreach (ToolStripTabItem tabItem in ribbonControlAdv1.Header.MainItems)
            {
                foreach (Control control in tabItem.Panel.Controls)
                {
                    var toolStrip = control as ToolStripEx;
                    if (toolStrip != null && (toolStrip.Items.Count == 0 & toolStrip != toolStripExMessages))
                    {
                        tabItem.Panel.Controls.Remove(toolStrip);
                    }
                }

                tabItem.Panel.Size = tabItem.Panel.PreferredSize;
            }

            //tabitems remove
            if (!PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyShiftCategoryPreferences) &&
                !PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyExtendedPreferences))
            {
                toolStripTabItemHome.Panel.Controls.Remove(toolStripExPreferences);
            }

            //set size for each existing toolstripEx at last
            foreach (Control control in toolStripTabItemHome.Panel.Controls)
            {
                var toolStrip = control as ToolStripEx;
                if (toolStrip != null)
                {
                    toolStrip.Size = toolStrip.PreferredSize;
                }
            }
        }

        private void LoadToolStripGalleryPreferences()
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            // Clears items before loading.
            teleoptiToolStripGalleryPreferences.Items.Clear();
            var image = imageList1.Images[2];

            // Loads templates to toolStripGalleryPreferences.
            if (PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyExtendedPreferences))
            {
                var galleryTemplates = SdkServiceHelper.SchedulingService.GetExtendedPreferenceTemplates(
                    StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson);

                foreach (var templateDto in galleryTemplates)
                {
                    if (preferenceView != null)
                        image = preferenceView.IsValid(templateDto) ? imageList1.Images[2] : imageList1.Images[3];
                    var galleryItem = new Common.Controls.ToolStripGallery.ToolStripGalleryItemEx
                    {
                        Image = image,
                        ImageTransparentColor = Color.Magenta,
                        Tag = templateDto,
                        Text = templateDto.Name
                    };
                    teleoptiToolStripGalleryPreferences.Items.Add(galleryItem);
                }
            }

            // Loads DayOffs to toolStripGalleryPreferences.
            foreach (DayOff dayOff in AgentScheduleStateHolder.Instance().DaysOff)
            {
                var galleryItem = new Common.Controls.ToolStripGallery.ToolStripGalleryItemEx
                {
                    Image =
                        imageList1.Images[0],
                    ImageTransparentColor =
                        Color.Magenta,
                    Tag = dayOff,
                    Text = dayOff.Name
                };
                teleoptiToolStripGalleryPreferences.Items.Add(galleryItem);
            }

            foreach (ShiftCategory category in AgentScheduleStateHolder.Instance().ShiftCategories)
            {
                var galleryItem = new Common.Controls.ToolStripGallery.ToolStripGalleryItemEx
                {
                    Image =
                        imageList1.Images[1],
                    ImageTransparentColor =
                        Color.Magenta,
                    Tag = category,
                    Text = category.Name
                };
                teleoptiToolStripGalleryPreferences.Items.Add(galleryItem);
            }

            foreach (var absence in AgentScheduleStateHolder.Instance().Absences)
            {
                var galleryItem = new Common.Controls.ToolStripGallery.ToolStripGalleryItemEx
                {
                    Image = imageList1.Images[4],
                    ImageTransparentColor = Color.Magenta,
                    Tag = absence,
                    Text = absence.Name
                };
                teleoptiToolStripGalleryPreferences.Items.Add(galleryItem);
            }

            _toolStripGalleryPreferencesLoaded = true;
        }

        void teleoptiToolStripGalleryPreferences_GalleryItemClicked(object sender, ToolStripGalleryItemEventArgs args)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null) return;
            Cursor = Cursors.WaitCursor;
            var shiftCategory = args.GalleryItem.Tag as ShiftCategory;
            var dayOff = args.GalleryItem.Tag as DayOff;
            var absence = args.GalleryItem.Tag as Absence;

            if (shiftCategory != null)
            {
                preferenceView.AddShiftCategoryPreference(shiftCategory);
            }
            else if (dayOff != null)
            {
                preferenceView.AddDayOffPreference(dayOff);
            }
            else if (absence != null)
            {
                preferenceView.AddAbsencePreference(absence);
            }
            else
            {
                preferenceView.AddTemplatePreference(args.GalleryItem.Tag as ExtendedPreferenceTemplateDto);
            }
            Cursor = Cursors.Default;
        }

        private void BuildReportsTooItems()
        {

            ICollection<MatrixReportInfoDto> reportList;
            try
            {
                reportList = SdkServiceHelper.LogOnServiceClient.GetMatrixReportInfo();
            }
            catch (FaultException)
            {
                reportList = new List<MatrixReportInfoDto>();
            }

            toolStripSplitButtonReport.DropDownItems.Clear();

            foreach (MatrixReportInfoDto reportDto in reportList)
            {
                ToolStripItem item = new ToolStripMenuItem();
                if (reportDto.ReportName.Length > 0)
                {
                    string resourceKey = reportDto.ReportName.Substring(2, reportDto.ReportName.Length - 2);
                    string localizedResourceText = Resources.ResourceManager.GetString(resourceKey);
                    if(string.IsNullOrEmpty(localizedResourceText)) localizedResourceText = reportDto.ReportName;
                    
                    item.Text = localizedResourceText;
                    item.Tag = reportDto;
                    item.Click += ToolStripMenuSomeReportClick;

                    toolStripSplitButtonReport.DropDownItems.Add(item);
                    
                }
            }

            //Check for permissions for MyReport.
            if (PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenMyReport))
            {
                //Add MyReport menu item.
                ToolStripItem myReportItem = new ToolStripMenuItem {Text = Resources.MyReport};
            	myReportItem.Click += MyReportItemClick;
                toolStripSplitButtonReport.DropDownItems.Add(myReportItem);
            }

            if (toolStripSplitButtonReport.DropDownItems.Count == 0)
            {
                toolStripExView.Items.Remove(toolStripSplitButtonReport);
                toolStripExView.Size = toolStripExView.PreferredSize;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void LoadAsm()
        {
            var control = panelSchedule.Controls[0] as ScheduleControl;
            if (control == null) return;

            _scheduleMessengerScreen = new ScheduleMessengerScreen(this, control.ScheduleView, _controller)
                                       	{
                                       		StartPosition = FormStartPosition.Manual,
                                       		WindowState = FormWindowState.Normal
                                       	};
        	IsMessengerLoaded = true;
        }

        private void officeButtonAbout_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            HidePreferenceTab();
            HideStudentAvailabilityTab();
            ShowRequestTab(false);
            var manager = new ScheduleToPdfManager();
            PersonDto person = AgentScheduleStateHolder.Instance().Person;
            var control = panelSchedule.Controls[0] as ScheduleControl;
            if (control == null) return;

            CultureInfo cultureUi = (person.UICultureLanguageId.HasValue
                                        ? CultureInfo.GetCultureInfo(person.UICultureLanguageId.Value)
										: CultureInfo.CurrentUICulture).FixPersianCulture();

        	CultureInfo culture = (person.CultureLanguageId.HasValue
        	                      	? CultureInfo.GetCultureInfo(person.CultureLanguageId.Value)
									: CultureInfo.CurrentCulture).FixPersianCulture();

            bool rightToLeft = cultureUi.TextInfo.IsRightToLeft;

            DateOnlyPeriod period = control.ScheduleView.LoadedPeriod;
            if (period.DayCount() < 7)
                return;
            ScheduleReportDetail detail = ScheduleReportDetail.All;
            if (period.DayCount() > 7)
                detail = ScheduleReportDetail.None;

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\Teleopti";
			manager.ExportIndividual(new List<PersonDto> { person }, period, AgentScheduleStateHolder.Instance(), rightToLeft, detail, this, true, folderPath, culture);
        }

        private void toolStripButtonPreference_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            HideStudentAvailabilityTab();
            ShowRequestTab(false);
            //Load Preference view
            //ToggleButtonEnabled("toolStripButtonExport", false);
            toolStripButtonExport.Enabled = false;
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Preference, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            
            SetUpPreferenceToolStrip();
            Cursor = Cursors.Default;
        }

        private void SetUpPreferenceToolStrip()
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null) return;

            if (!_toolStripGalleryPreferencesLoaded)
                LoadToolStripGalleryPreferences();
             
            toolStripTabItemPreferences.Visible = true;
            toolStripTabItemPreferences.Checked = true;
            var dateOnly = new DateOnly(preferenceView.Presenter.FirstDateOfPeriod);
            var dateOnlyDto = new DateOnlyDto { DateTime = dateOnly.Date };
            var helper = new PlanningTimeBankHelper(SdkServiceHelper.SchedulingService);
            toolStripExTimeBank.Visible = helper.SetIsAllowed(PermissionService.Instance(),
                                                              ApplicationFunctionHelper.Instance(),
                                                              StateHolder.Instance.StateReader.SessionScopeData.
                                                                  LoggedOnPerson, dateOnlyDto, preferenceView.Presenter);
        }
        
        private void HidePreferenceTab()
        {
            toolStripTabItemPreferences.Visible = false;
            toolStripTabItemPreferences.Checked = false;
        }

        private void HideStudentAvailabilityTab()
        {
            toolStripTabItemStudentAvailability.Visible = false;
            toolStripTabItemStudentAvailability.Checked = false;
        }

        private void toolStripButtonPreviousPeriod_Click(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null) return;
            Cursor = Cursors.WaitCursor;
            preferenceView.Presenter.GetPreviousPeriod();
            SetUpPreferenceToolStrip();
            //LoadToolStripGalleryPreferences();
            Cursor = Cursors.Default;
        }

        private void toolStripButtonNextPeriod_Click(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null) return;
            Cursor = Cursors.WaitCursor;
            preferenceView.Presenter.GetNextPeriod();
            SetUpPreferenceToolStrip();
            //LoadToolStripGalleryPreferences();
            Cursor = Cursors.Default;
        }

        private void toolStripButtonValidate_Click(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null) return;
            Cursor = Cursors.WaitCursor;
            preferenceView.Presenter.ReloadPeriod();
            Cursor = Cursors.Default;
        }

        public void ToggleButtonEnabled(string controlName, bool enabled)
        {
            if (controlName == "clipboardControl")
                _clipboardControl.ToolStripSplitButtonPaste.Enabled = enabled;
            else if (controlName == "clipboardControlStudentAvailability")
                _clipboardControlStudentAvailability.ToolStripSplitButtonPaste.Enabled = enabled;
            else if (controlName == "toolStripButtonValidate")
            {
                toolStripButtonValidate.Enabled = enabled;
            }
            else if (controlName == "toolStripButtonSAValidate")
            {
                toolStripButtonSAValidate.Enabled = enabled;
            }
            else if (controlName == "toolStripButtonMustHave")
                toolStripButtonMustHave.Enabled = enabled;
            else if (controlName == "toolStripButtonExport")
            {
                toolStripButtonExport.Enabled = enabled;
                _lastExportButtonState = enabled;
            }
            else if (controlName == "refresh")
            {
                LoadToolStripGalleryPreferences();
            }
        }

        public void ToggleButtonChecked(string controlName, bool? isChecked)
        {
        	if (controlName != "toolStripButtonMustHave") return;
        	if (isChecked.HasValue)
        		toolStripButtonMustHave.Checked = isChecked.Value;
        	else
        		toolStripButtonMustHave.CheckState = CheckState.Indeterminate;
        }

    	private void toolStripButtonMustHave_Click(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
        	if (preferenceView == null) return;
        	bool mustHave = toolStripButtonMustHave.Checked;
        	Cursor = Cursors.WaitCursor;
        	preferenceView.ToggleMustHave(mustHave);
        	Cursor = Cursors.Default;
        }

        public void SetMustHaveText(string mustHaveInfo)
        {
            toolStripButtonMustHave.Text = Resources.MustHaveCapitalized + mustHaveInfo;
        }

        public void ShowRightPanel(bool show)
        {
            splitContainerAdv1.Panel2Collapsed = !show;
        }

        private void toolStripButtonAccounts_Click(object sender, EventArgs e)
        {
            HidePreferenceTab();
            HideStudentAvailabilityTab();
            ShowRequestTab(false);
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.Accounts, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            toolStripButtonExport.Enabled = false;
        }

        private void toolStripButtonModifyRequest_Click(object sender, EventArgs e)
        {
            var view = panelSchedule.Controls[0] as RequestMasterView;
            if (view != null) view.ModifySelectedRequest();
        }

        #region DontRepeatYourself

        private void StartMessageBrokerListener()
        {
            RegisterForMessageBrokerEvents();
        }

        private void RegisterForMessageBrokerEvents()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsConnected)
            {
                try
                {
                	var details = StateHolder.Instance.State.SessionScopeData;
                	StateHolder.Instance.MessageBroker.RegisterEventSubscription(details.DataSource.Name,
                	                                                             details.BusinessUnit.Id.GetValueOrDefault(), OnEventMessageHandler, typeof(IPushMessageDialogue));
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(details.DataSource.Name,
																				 details.BusinessUnit.Id.GetValueOrDefault(), OnEventMessageHandler, typeof(IPersonRequest));
                }
                catch (RemotingException e)
                {
                    // TODO: how should we handle MB exceptions ? 
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventMessageArgs>(OnEventMessageHandler), sender, e);
            }
            else
            {
                _controller.MessageChanged(e);
            }
        }

        public void UnregisterMessageBrokerSubscriptions()
        {
            if (StateHolder.Instance.MessageBroker != null && StateHolder.Instance.MessageBroker.IsConnected)
            {
                try
                {
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
                }
                catch (RemotingException exp)
                {
                    // TODO: how should we handle MB exceptions ? 
                    Console.WriteLine(exp.Message);
                }
            }
        }

        #endregion

        private void teleoptiToolStripGalleryPreferences_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Tag as bool? == true) return;
            _currentTemplate = e.ClickedItem.Tag;
            SetupGalleryItem(e.ContextMenuStrip);
        }

        private void SetupGalleryItem(ContextMenuStrip contextMenu)
        {
            if (!(_currentTemplate is ExtendedPreferenceTemplateDto)) return;
            ToolStripItem itemRemove = new ToolStripMenuItem { Text = Resources.Delete };
            itemRemove.Click += itemRemove_Click;
            contextMenu.Items.Add(itemRemove);

            ToolStripItem itemRename = new ToolStripMenuItem { Text = Resources.Rename };
            itemRename.Click += itemRename_Click;
            contextMenu.Items.Add(itemRename);
        }

        private void itemRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = sender as ToolStripItem;
            if (selectedItem == null) return;
            var theTemplate = _currentTemplate as ExtendedPreferenceTemplateDto;
            SdkServiceHelper.SchedulingService.DeleteExtendedPreferenceTemplate(theTemplate);
            var itemToDelete = FindItemByTag(theTemplate);
            if (itemToDelete != null)
            {
                teleoptiToolStripGalleryPreferences.Items.Remove(itemToDelete);
            }
        }

        private ToolStripGalleryItem FindItemByTag(object tag)
        {
            ToolStripGalleryItem foundItem = null;
            foreach (ToolStripGalleryItem item in teleoptiToolStripGalleryPreferences.Items)
            {
                if (item != null && item.Tag==tag)
                {
                    foundItem = item;
                    break;
                }
            }
            return foundItem;
        }

        private void itemRename_Click(object sender, EventArgs e)
        {
            var selectedItem = sender as ToolStripItem;
            if (selectedItem == null) return;
            var createTemplate = new CreateExtendedPreferencesTemplate();
            var dto = _currentTemplate as ExtendedPreferenceTemplateDto;
            if (dto == null) return;

            createTemplate.InputName = dto.Name;
            if (DialogResult.OK != createTemplate.ShowDialog(this)) return;
                dto.Name = createTemplate.InputName;
            var itemToRename = FindItemByTag(dto);
            if (itemToRename!=null)
            {
                itemToRename.Tag = dto;
                itemToRename.Text = dto.Name;

                teleoptiToolStripGalleryPreferences.Invalidate();
            }
            SdkServiceHelper.SchedulingService.SaveExtendedPreferenceTemplate(dto);
        }

        private void ToolStripButtonPlanningTimeBankClick(object sender, EventArgs e)
        {
            var preferenceView = panelSchedule.Controls[0] as PreferenceView;
            if (preferenceView == null)
                return;
            // TODO convert to correct time zone
            var dateOnly = new DateOnly(preferenceView.Presenter.FirstDateOfPeriod);
            var dateOnlyDto = new DateOnlyDto {DateTime = dateOnly.Date};
            var helper = new PlanningTimeBankHelper(SdkServiceHelper.SchedulingService);
            var model = helper.GetPlanningTimeBankModel(StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson,
                                                   dateOnlyDto);
            if(!model.CanSetBalanceOut)
                return;

            using (var form = new PlanningTimeBankForm(helper, model))
            {
                if (DialogResult.OK == form.ShowDialog(this))
                    preferenceView.Presenter.ReloadPeriod();
            }
        }

        private void toolStripButtonViewStudentAvailability_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            HidePreferenceTab();
            ShowRequestTab(false);
            toolStripButtonExport.Enabled = false;
            Control hosted = ViewConstructor.Instance.BuildPortalView(ViewType.StudentAvailable, PanelMain, this);
            ActiveControl = hosted;
            SetupHelpContext(hosted);
            SetUpStudentAvailabilityToolStrip();
            Cursor = Cursors.Default;
        }

        private void SetUpStudentAvailabilityToolStrip()
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView == null) return;

            toolStripTabItemStudentAvailability.Visible = true;
            toolStripTabItemStudentAvailability.Checked = true;
        }

        private void toolStripButtonSAPreviousPeriod_Click(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView == null) return;
            Cursor = Cursors.WaitCursor;
            studentAvailabilityView.Presenter.GetPreviousPeriod();
            SetUpStudentAvailabilityToolStrip();
            Cursor = Cursors.Default;
        }

        private void toolStripButtonSANextPeriod_Click(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView == null) return;
            Cursor = Cursors.WaitCursor;
            studentAvailabilityView.Presenter.GetNextPeriod();
            SetUpPreferenceToolStrip();
            SetUpStudentAvailabilityToolStrip();
            Cursor = Cursors.Default;
        }

        private void toolStripButtonSAValidate_Click(object sender, EventArgs e)
        {
            var studentAvailabilityView = panelSchedule.Controls[0] as AvailabilityView;
            if (studentAvailabilityView == null) return;
            Cursor = Cursors.WaitCursor;
            studentAvailabilityView.Presenter.ReloadPeriod();
            Cursor = Cursors.Default;
        }
    }
}
