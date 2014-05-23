using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Requests;
using Teleopti.Ccc.AgentPortal.Requests.FormHandler;
using Teleopti.Ccc.AgentPortal.Requests.ShiftTrade;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    public partial class ScheduleControl : BaseUserControl
    {
	    private readonly IToggleButtonState _parent;
	    private readonly ILegendLoader _legendLoader;

	    public CalendarViewType SelectedScheduleView
        {
            get { return (CalendarViewType)tabControlAdvMainTab.SelectedIndex; }
        }

	    public AgentScheduleView ScheduleView { get; set; }

	    public event EventHandler<EventArgs> ViewChanged;

        public ScheduleControl(IToggleButtonState parent, ILegendLoader legendLoader)
        {
			InitializeComponent();

			_parent = parent;
			_legendLoader = legendLoader;
            
            InitalizeScheduleControlContextMenu();
            InitializeScheduleControl();
        }

        private void ScheduleControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode) SetTexts();
            try
            {
                Cursor = Cursors.WaitCursor;
                toolStripMenuItem60Mins.Visible = false;
                tabControlAdvMainTab.ContextMenu = new ContextMenu(); //Block right click in tabControl
                scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
            }
            catch (System.Data.DataException ex)
            {
                string dataErrorMessage = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.ErrorOccuredWhenAccessingTheDataSource + "\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(dataErrorMessage, UserTexts.Resources.AgentPortal);
            }
            catch (FaultException ex)
            {
                string communicationErrorMessage = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.CommunicationErrorEndPoint + "\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(communicationErrorMessage, UserTexts.Resources.AgentPortal);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void tabControlAdvMainTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            
            switch(tabControlAdvMainTab.SelectedIndex)
            {
                case 0:
                    SetScheduleCurrentView(ScheduleViewType.Day);
                    scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 1:
                    SetScheduleCurrentView(ScheduleViewType.Week);
                    scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 2:
                    SetScheduleCurrentView(ScheduleViewType.Month);
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 3:
                    scheduleTeamView.InitializeScheduleTeamView();
                    SetScheduleCurrentView(ScheduleViewType.CustomWeek);
                    if (_parent != null)
                        _parent.ToggleButtonEnabled("toolStripButtonExport", false);
                    break;
            }
            Cursor = Cursors.Default;
        }

        public void RefreshSchedule()
        {
            Cursor = Cursors.WaitCursor;

            switch (tabControlAdvMainTab.SelectedIndex)
            {
                case 0:
                    SetScheduleCurrentView(ScheduleViewType.Day);
                    scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 1:
                    SetScheduleCurrentView(ScheduleViewType.Week);
                    scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 2:
                    SetScheduleCurrentView(ScheduleViewType.Month);
                    if (ViewChanged != null)
                        ViewChanged(this, new EventArgs());
                    break;
                case 3:
                    scheduleTeamView.InitializeScheduleTeamView();
                    SetScheduleCurrentView(ScheduleViewType.CustomWeek);
                    if (_parent != null)
                        _parent.ToggleButtonEnabled("toolStripButtonExport", false);
                    break;
            }
            Cursor = Cursors.Default;

        }
        /// <summary>
        /// Handles the ItemClick event of the contextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/18/2008
        /// </remarks>
        private void contextMenuStrip_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem selectedItem = e.ClickedItem as ToolStripMenuItem;

            if (selectedItem == null || selectedItem.Tag == null) //if toolstripSeperator clicked
            {
                return;
            }
            var resolution = (int)e.ClickedItem.Tag;

            scheduleControlMain.Appearance.DivisionsPerHour = resolution;
            UncheckAllResolutionMenuItems();
            selectedItem.Checked = true;

            ScheduleView.SetResolution( resolution);
            AgentPortalSettingsHelper.SaveSettings(new SaveAgentPortalSettingsCommandDto { Resolution = resolution});
        }
   
        /// <summary>
        /// Handles the Click event of the toolStripMenuItemDefaultColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/18/2008
        /// </remarks>
        private void toolStripMenuItemDefaultColor_Click(object sender, EventArgs e)
        {
            toolStripMenuItemActivityColor.Checked = false;
            ScheduleAppointmentFactory.ColorTheme = ScheduleAppointmentColorTheme.DefaultColor;
            ScheduleView.SetColorTheme(ScheduleAppointmentColorTheme.DefaultColor);
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemSysColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/18/2008
        /// </remarks>
        private void toolStripMenuItemSysColor_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSystemColor.Checked = false;
            ScheduleAppointmentFactory.ColorTheme = ScheduleAppointmentColorTheme.SystemColor;
            ScheduleView.SetColorTheme(ScheduleAppointmentColorTheme.SystemColor);
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-08-28
        /// </remarks>
        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            ScheduleView.DeleteScheduleAppointments();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemOpen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-08-28
        /// </remarks>
        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            if (scheduleControlMain.ClickedScheduleAppointment == null)
                return;

            PersonRequestDto personRequestDto = scheduleControlMain.ClickedScheduleAppointment.Tag as PersonRequestDto;
            if (personRequestDto != null)
            {
                new PersonRequestFormHandler(this).ShowRequestScreen(personRequestDto);
                ScheduleView.Refresh();
            }
        }
      
        /// <summary>
        /// Handles the Click event of the toolStripMenuItemNewAbsenceRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-10-09
        /// </remarks>
        private void toolStripMenuItemNewAbsenceRequest_Click(object sender, EventArgs e)
        {
            DateTime startDateTime;
            DateTime endDateTime;

            if (scheduleControlMain.ScheduleType != ScheduleViewType.Month)
            {
                GetStartAndEndTimes(out startDateTime, out endDateTime);
            }else
            {
                //All Day
                GetAllDayDateTimes(out startDateTime, out endDateTime);
            }
            DateTimePeriodDto period = HelperFunctions.CreateDateTimePeriodDto(startDateTime, endDateTime);
            AbsenceRequestView absenceRequestView = new AbsenceRequestView(period);
            absenceRequestView.StartPosition = FormStartPosition.CenterScreen;
            absenceRequestView.ShowDialog(this);
            ScheduleView.Refresh();
        }

        private void GetAllDayDateTimes(out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = scheduleControlMain.ClickedDate.Date;
            endDateTime = scheduleControlMain.ClickedDate.Date.AddDays(1).AddMinutes(-1);
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemNewShiftTrade control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-09
        /// </remarks>
        private void toolStripMenuItemNewShiftTrade_Click(object sender, EventArgs e)
        {
            DateTime tradeDate = scheduleTeamView.StartDateForTeamView;
            PersonDto personToTradeWith = scheduleTeamView.LastRightClickedPerson;
            PersonDto loggedPerson = SdkServiceHelper.LogOnServiceClient.GetLoggedOnPerson();
            createAndOpenShiftTradeForm(tradeDate, personToTradeWith, loggedPerson);
        }

        private void createAndOpenShiftTradeForm(DateTime tradeDate, PersonDto personToTradeWith, PersonDto loggedPerson)
        {
            DateOnlyDto dateOnlyDto = new DateOnlyDto { DateTime = tradeDate };
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto { DateFrom = dateOnlyDto, DateTo = dateOnlyDto, PersonFrom = loggedPerson, PersonTo = personToTradeWith };
            PersonRequestDto personRequestDto = SdkServiceHelper.SchedulingService.CreateShiftTradeRequest(loggedPerson, "", "", new[] { shiftTradeSwapDetailDto });
            PersonDto loggedOnPerson = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            ShiftTradeModel model = new ShiftTradeModel(SdkServiceHelper.SchedulingService, personRequestDto, loggedOnPerson, tradeDate);

            using (ShiftTradeView shiftTradeView = new ShiftTradeView(model, false))
            {
                if (shiftTradeView.ShowDialog(this) == DialogResult.OK)
                {
                    SdkServiceHelper.SchedulingService.SavePersonRequest(personRequestDto);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemNewTextRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-14
        /// </remarks>
        private void toolStripMenuItemNewTextRequest_Click(object sender, EventArgs e)
        {
            DateTime startTime;
            DateTime endTime;
            GetStartAndEndTimes(out startTime, out endTime);
            DateTimePeriodDto period = HelperFunctions.CreateDateTimePeriodDto(startTime, endTime);
            TextRequestView requestScreen = new TextRequestView(period, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson, SdkServiceHelper.SchedulingService);
            requestScreen.StartPosition = FormStartPosition.CenterScreen;
            requestScreen.ShowDialog(this);
            ScheduleView.Refresh();
        }

        /// <summary>
        /// Called when [event message handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventMessageArgs>(OnEventMessageHandler),sender,e);
            }
            else
            {
                if (IsDisposed) return;

                if ((e.Message.DomainUpdateType == DomainUpdateType.Insert) ||
                    (e.Message.DomainUpdateType == DomainUpdateType.Update) ||
                    (e.Message.DomainUpdateType == DomainUpdateType.Delete))
                {
                    if (AgentScheduleStateHolder.Instance().CanVisualize(ScheduleAppointmentTypes.Request))
                    {
                        ScheduleView.SetDataSource();
                    }
                }
            }
        }

        public ScheduleViewType ScheduleType
        {
            get { return scheduleControlMain.ScheduleType; }
        }

        /// <summary>
        /// Handles the ScheduleTypeChanged event of the scheduleControlMain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        private void scheduleControlMain_ScheduleTypeChanged(object sender, EventArgs e)
        {
            tabControlAdvMainTab.SelectedIndexChanged -= tabControlAdvMainTab_SelectedIndexChanged;

            switch (scheduleControlMain.ScheduleType)
            {
                case ScheduleViewType.Week:
                    tabControlAdvMainTab.SelectedIndex = (int)CalendarViewType.Workweek;
                    break;
                case ScheduleViewType.CustomWeek:
                    tabControlAdvMainTab.SelectedIndex = (int)CalendarViewType.Workweek;
                    break;
                case ScheduleViewType.Day:
                    tabControlAdvMainTab.SelectedIndex = (int)CalendarViewType.Day;
                    break;
                case ScheduleViewType.Month:
                    tabControlAdvMainTab.SelectedIndex = (int)CalendarViewType.Month;
                    break;
            }

            tabControlAdvMainTab.SelectedIndexChanged += tabControlAdvMainTab_SelectedIndexChanged;
        }

        /// <summary>
        /// Initializes the schedule control.
        /// </summary>
        /// <remarks>
        /// Created by: MuhamadR
        /// Created date: 2008-03-06
        /// </remarks>
        private void InitializeScheduleControl()
        {
            if (!AgentScheduleStateHolder.Instance().Initialized)
            {
                AgentScheduleStateHolder.Instance().Initialize(
                    StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson);
            }
            
            //Events 
            scheduleControlMain.ScheduleTypeChanged += scheduleControlMain_ScheduleTypeChanged;  

            //Set contextmenu

            contextmenuStripExScheduleControl.Opening += new CancelEventHandler(contextmenuStripExScheduleControl_Opening);
            ContextMenuStrip = contextmenuStripExScheduleControl;
            scheduleControlMain.ScheduleAppointmentClick += (s, e) =>
                                                                {
                                                                    if (e.ClickType == ScheduleAppointmentClickType.RightClick)
                                                                    {
                                                                        if (tabControlAdvMainTab.SelectedIndex != 3)
                                                                        {
                                                                            BuildContextMenu();
                                                                            contextmenuStripExScheduleControl.Show(Cursor.Position);
                                                                        }
                                                                    }
                                                                };

            ScheduleView = new AgentScheduleView(scheduleControlMain, AgentScheduleStateHolder.Instance(), _legendLoader);           
            ScheduleView.InitializeScheduleControl();
            ScheduleView.SetScheduleControlEventHandlers();
            ScheduleView.ScheduleControlHost.AllowAdjustAppointmentsWithMouse = false;
            SetScheduleCurrentView(ScheduleViewType.Week);
			ScheduleView.ScheduleControlHost.Resize += ScheduleControlHost_Resize;

            // Start Message Broker Listener
            StartMessageBrokerListener();
            scheduleControlMain.GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
        }

		void ScheduleControlHost_Resize(object sender, EventArgs e)
		{
			RefreshSchedule();

		}

        void contextmenuStripExScheduleControl_Opening(object sender, CancelEventArgs e)
        {
            if (tabControlAdvMainTab.SelectedIndex == 3)
            {
                BuildContextMenu();
            }
        }

        /// <summary>
        /// Initalizes the schedule control context menu.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        private void InitalizeScheduleControlContextMenu()
        {
            toolStripMenuItem60Mins.Tag = ScheduleResolutionType.SixtyMinutes;
            toolStripMenuItem30Mins.Tag = ScheduleResolutionType.ThirtyMinutes;
            toolStripMenuItem15Mins.Tag = ScheduleResolutionType.FifteenMinutes;
            toolStripMenuItem10Mins.Tag = ScheduleResolutionType.TenMinutes;
            toolStripMenuItem6Mins.Tag = ScheduleResolutionType.SixMinutes; 
            toolStripMenuItem5Mins.Tag = ScheduleResolutionType.FiveMinutes;
        }

        /// <summary>
        /// Gets the start and end times.
        /// </summary>
        /// <param name="startDateTime">The start time.</param>
        /// <param name="endDateTime">The end time.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 10/15/2008
        /// </remarks>
        private void GetStartAndEndTimes(out DateTime startDateTime, out DateTime endDateTime)
        {
            GridModelSelections gridModelSelections = scheduleControlMain.GetScheduleHost().Model.Selections;
            GridRangeInfoList selectedRowRange = gridModelSelections.GetSelectedRows(false, true);
            int divisionPerHour = scheduleControlMain.Appearance.DivisionsPerHour;
            startDateTime = DateTime.MinValue;
            endDateTime = DateTime.MinValue;
            //for multiple or single selection
            if (selectedRowRange.Count > 0)
            {
                const int rowOffset = 2;
                if (selectedRowRange[0].Top == 1) //This is the top Blue box 
                {
                    GetAllDayDateTimes(out startDateTime,out endDateTime);
                }
                else
                {
                    int timeStartRow = selectedRowRange[0].Top - rowOffset;
                    int timeEndRow = selectedRowRange[0].Bottom - rowOffset;
                    int totalMinutes = ((timeEndRow - timeStartRow) + 1) * (60 / divisionPerHour);
                    int startHour = timeStartRow / divisionPerHour;
                    int startMinute = (timeStartRow % divisionPerHour) * (60 / divisionPerHour);

                    startDateTime = scheduleControlMain.ClickedDate.Date.AddHours(startHour).AddMinutes(startMinute);
                    endDateTime = startDateTime.AddMinutes(totalMinutes);
                }
            }
            else //just for mouse right click without a selection
            {
                if (scheduleControlMain.SelectedDates.Count > 0)
                {
                    DateTime selectedDate = scheduleControlMain.SelectedDates[0] == DateTime.MinValue ?
                           DateTime.Today : scheduleControlMain.ClickedDate.Date;
                    startDateTime = selectedDate.Date;
                    endDateTime = selectedDate.Date;
                }
                else
                {
                    startDateTime = ScheduleView.ScheduleControlHost.ClickedDate.Date;
                    endDateTime = ScheduleView.ScheduleControlHost.ClickedDate.AddDays(1).AddMinutes(-1);
                }
            }
        }

        /// <summary>
        /// Builds the context menu.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void BuildContextMenu()
        {
            PersonDto lastClickedPerson = scheduleTeamView.LastRightClickedPerson;
            PersonDto loggedOnPerson = SdkServiceHelper.LogOnServiceClient.GetLoggedOnPerson();

            // ui dependecies
            var isScheduleAppointmentSelected = scheduleControlMain.ClickedScheduleAppointment != null;
            var isTimelineSelected = scheduleControlMain.IsTimelineSelected;
            if (scheduleTeamView.Visible)
                isTimelineSelected = false;
            var isHeaderSelected = scheduleControlMain.IsHeaderSelected;
            var isScheduleGridSelected = !isTimelineSelected && !isHeaderSelected;
            var isInMonthView = scheduleControlMain.ScheduleType == ScheduleViewType.Month ? true : false;
            var isDefaultColorTheme = (AgentScheduleStateHolder.Instance().ScheduleAppointmentColorTheme == ScheduleAppointmentColorTheme.DefaultColor);
            //var iClickedMySelf = lastClickedPerson.Id == loggedOnPerson.Id;

            // permissions
            var canOpen = scheduleControlMain.ClickedScheduleAppointment != null && scheduleControlMain.ClickedScheduleAppointment.AllowOpen;
            var canDelete = scheduleControlMain.ClickedScheduleAppointment != null && scheduleControlMain.ClickedScheduleAppointment.AllowDelete;
            var permissionService = PermissionService.Instance();
            var canCreateTextRequests = permissionService.IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateTextRequest) && !scheduleTeamView.Visible;
            var canCreateAbsenceRequests = permissionService.IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateAbsenceRequest) && !scheduleTeamView.Visible;
            var canCreateShiftTradeRequests = permissionService.IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateShiftTradeRequest) && scheduleTeamView.Visible;
            var shiftTradeInSameTimeZone = false;
        	var noAbsenceInShifts = true;

            bool iClickedMySelf = false;

            if (lastClickedPerson != null)
            {
                iClickedMySelf = lastClickedPerson.Id == loggedOnPerson.Id;
            }

            if (scheduleTeamView.StartDateForTeamView >= DateTime.Today)
            {
                if (SelectedScheduleView == CalendarViewType.Team && (lastClickedPerson != null))
                {
                    if (!iClickedMySelf && lastClickedPerson.TimeZoneId == loggedOnPerson.TimeZoneId) //Only allow trades with another person in same time zone
                    {
                        shiftTradeInSameTimeZone = true;
                    }
                }
            }

			if (SelectedScheduleView == CalendarViewType.Team && lastClickedPerson != null)
			{
				noAbsenceInShifts = !ShiftContainsAbsence();
			}

            // determine what gets shown
            var showOpen = canOpen && isScheduleAppointmentSelected && isScheduleGridSelected;
            var showNewAbsenceRequests = canCreateAbsenceRequests && isScheduleGridSelected;
            var showNewShiftTrade = canCreateShiftTradeRequests && isScheduleGridSelected;
            var enableNewShiftTrade = shiftTradeInSameTimeZone && noAbsenceInShifts;
            var showNewTextRequest = canCreateTextRequests && isScheduleGridSelected;
            var showDelete = canDelete && isScheduleAppointmentSelected && isScheduleGridSelected;
            var showActivityColor = isScheduleGridSelected;
            var showResolution = !isInMonthView && isTimelineSelected && !isHeaderSelected;
            // determine which sections gets shown...
            var showSectionOpenNew = (showOpen || showNewAbsenceRequests || showNewShiftTrade || showNewTextRequest);
            var showSectionDelete = showDelete;
            var showSectionActivityColor = showActivityColor;
            var showSectionResolution = showResolution;
            // ...to be able to determine which separators gets show
            var showSeparatorDelete = 
                (showSectionOpenNew) && 
                showDelete;
            var showSeparatorActivity = 
                (showSectionOpenNew || showSectionDelete) &&
                showSectionActivityColor;
            var showSeparatorResolution = 
                (showSectionOpenNew || showSectionDelete || showSectionActivityColor) &&
                showSectionResolution;

            // set up context menus
            toolStripMenuItemOpen.Visible = showOpen;
            toolStripMenuItemNewAbsenceRequest.Visible = showNewAbsenceRequests;
            toolStripMenuItemNewShiftTrade.Visible = showNewShiftTrade;
            toolStripMenuItemNewShiftTrade.Enabled = enableNewShiftTrade;
            toolStripMenuItemShiftTradeFilter.Visible = showNewShiftTrade;
            toolStripMenuItemNewTextRequest.Visible = showNewTextRequest;
            
            toolStripSeparatorDelete.Visible = showSeparatorDelete;
            toolStripMenuItemDelete.Visible = showDelete;

            toolStripSeparatorActivityColor.Visible = showSeparatorActivity;
            toolStripMenuItemActivityColor.Visible = showActivityColor;

            toolStripSeparatorResolutions.Visible = showSeparatorResolution;
            //toolStripMenuItem60Mins.Visible = showResolution;
            toolStripMenuItem30Mins.Visible = showResolution;
            toolStripMenuItem15Mins.Visible = showResolution;
            toolStripMenuItem10Mins.Visible = showResolution;
            toolStripMenuItem6Mins.Visible = showResolution;
            toolStripMenuItem5Mins.Visible = showResolution;
        	if (showResolution)
        		CheckCorrectResolutionMenuItem();

        	toolStripMenuItemSystemColor.Checked = !isDefaultColorTheme;
            toolStripMenuItemDefaultColor.Checked = isDefaultColorTheme;
        }

    	private void CheckCorrectResolutionMenuItem()
    	{
			switch ((ScheduleResolutionType)ScheduleView.Resolution)
    		{
    			case ScheduleResolutionType.SixtyMinutes:
					toolStripMenuItem60Mins.Checked = true;
					break;
				case ScheduleResolutionType.ThirtyMinutes:
					toolStripMenuItem30Mins.Checked = true;
					break;
				case ScheduleResolutionType.TenMinutes:
					toolStripMenuItem10Mins.Checked = true;
					break;
				case ScheduleResolutionType.SixMinutes:
					toolStripMenuItem6Mins.Checked = true;
					break;
				case ScheduleResolutionType.FiveMinutes:
					toolStripMenuItem5Mins.Checked = true;
					break;
				default:
    				toolStripMenuItem15Mins.Checked = true;
    				break;
    		}
    	}

    	private bool ShiftContainsAbsence()
		{
			var tradeDate = scheduleTeamView.StartDateForTeamView;
			var personToTradeWith = scheduleTeamView.LastRightClickedPerson;
			var loggedOnPerson = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
			var dateOnlyDto = new DateOnlyDto { DateTime = tradeDate };
    	    var schedule =
    	        SdkServiceHelper.SchedulingService.GetSchedulesByQuery(new GetSchedulesByPersonQueryDto
    	            {
    	                PersonId = personToTradeWith.Id.GetValueOrDefault(),
    	                StartDate = dateOnlyDto,
    	                EndDate = dateOnlyDto,
    	                TimeZoneId = loggedOnPerson.TimeZoneId
    	            });

    	    return schedule.SelectMany(scheduleDay => scheduleDay.ProjectedLayerCollection).Any(projectedLayerDto => projectedLayerDto.IsAbsence);
		}


        /// <summary>
        /// Uncecks all resolution menu items.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        private void UncheckAllResolutionMenuItems()
        {
            toolStripMenuItem60Mins.Checked=false;
            toolStripMenuItem30Mins.Checked=false;
            toolStripMenuItem15Mins.Checked=false;
            toolStripMenuItem10Mins.Checked=false;
            toolStripMenuItem6Mins.Checked=false;
            toolStripMenuItem5Mins.Checked=false;
        }

        /// <summary>
        /// Starts the message borker listner.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        private void StartMessageBrokerListener()
        {
            RegisterForMessageBrokerEvents();
        }

        /// <summary>
        /// Registers for message broker events.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-11
        /// </remarks>
        private void RegisterForMessageBrokerEvents()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsConnected)
            {
                try
                {
                	var details = StateHolder.Instance.State.SessionScopeData;
                    StateHolder.Instance.MessageBroker.RegisterEventSubscription(details.DataSource.Name,details.BusinessUnit.Id.GetValueOrDefault(), OnEventMessageHandler,
                                                         typeof(IPersonRequest));
                }
                catch (RemotingException)
                {
                    // TODO: how should we handle MB exceptions ? 
                    // TODO: Unregister!
                }
            }
        }

        public void UnregisterForMessageBrokerEvents()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsConnected)
            {
                try
                {
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
                }
                catch (RemotingException)
                {
                    // TODO: how should we handle MB exceptions ? 
                    // TODO: Unregister!
                }
            }
        }

        ///// <summary>
        ///// Sets the schedule current view.
        ///// </summary>
        ///// <param name="currentView">The current view.</param>
        ///// <remarks>
        ///// Created by: Sumedah
        ///// Created date: 2008-12-11
        ///// </remarks>
        public void SetScheduleCurrentView(ScheduleViewType currentView)
        {
            switch (currentView)
            {
                case ScheduleViewType.Day:
                    scheduleControlMain.Visible = true;
                    scheduleTeamView.Visible = false;
                    scheduleControlMain.PerformSwitchToScheduleViewTypeClick(ScheduleViewType.Day);
                    //scheduleControlMain.GetScheduleHost().SwitchTo(ScheduleViewType.Day, true);
                    scheduleControlMain.Appearance.ScheduleAppointmentTipsEnabled = true;
                    break;
                case ScheduleViewType.Week:
                    scheduleControlMain.Visible = true;
                    scheduleTeamView.Visible = false;
                    scheduleControlMain.SetSevenDayWeek();
                    break;
                case ScheduleViewType.Month:
                    scheduleControlMain.Visible = true;
                    scheduleTeamView.Visible = false;
                    //scheduleControlMain.GetScheduleHost().SwitchTo(ScheduleViewType.Month, true);
                    scheduleControlMain.PerformSwitchToScheduleViewTypeClick(ScheduleViewType.Month);
                    scheduleControlMain.Appearance.ScheduleAppointmentTipsEnabled = false;
                    break;
                default:
                    scheduleTeamView.Visible = true;
                    scheduleControlMain.Visible = false;
                    scheduleControlMain.ClickedScheduleAppointment = null;
                    scheduleTeamView.SetDateSelection(scheduleControlMain.Calendar.SelectedDates);
                    break;
            }
        }

        private void toolStripMenuItemShiftTradeFilter_Click(object sender, EventArgs e)
        {
            toolStripMenuItemShiftTradeFilter.Checked = !toolStripMenuItemShiftTradeFilter.Checked;
            scheduleTeamView.FilterPeopleForShiftTrade = toolStripMenuItemShiftTradeFilter.Checked;
        }
    }
}
