﻿using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Schedule;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings.Overview
{
    public partial class MeetingOverviewView : BaseRibbonForm, IMeetingOverviewView
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly CalendarAndTextPanel _calendarAndTextPanel;
        private readonly MeetingsScheduleProvider _dataProvider;
        private readonly IMeetingOverviewFilter _meetingOverviewFilter;

        private IScheduleAppointment _selectedItem;
        private readonly Control _nextButton;
        private readonly Control _previousButton;
        private DateTime _clickedDate;


        public MeetingOverviewView(IEventAggregator eventAggregator, MeetingsScheduleProvider dataProvider,IMeetingOverviewFilter meetingOverviewFilter)
        {
            _eventAggregator = eventAggregator;
            _dataProvider = dataProvider;
            _meetingOverviewFilter = meetingOverviewFilter;

            InitializeComponent();
            SetTexts();
            _eventAggregator.GetEvent<PersonSelectionFormHideEvent>().Subscribe(unCheckFilter);

            UserTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            ScheduleGrid.DisplayStrings[4] = "";//hide the text area in all day area in day view
            ScheduleGrid.DisplayStrings[3] = "";
            scheduleControl1.Appearance.WorkWeekHeaderFormat = "d dddd";

            scheduleControl1.Visible = false;
            // to set visible in CustomWeek
            _nextButton = scheduleControl1.CaptionPanel.Controls[1];
            _previousButton = scheduleControl1.CaptionPanel.Controls[2];

            _dataProvider.CreateListObjectList();

            _calendarAndTextPanel = new CalendarAndTextPanel();
            scheduleControl1.AddControlToNavigationPanel(_calendarAndTextPanel);
            resizeCalendarToNavigationPanel();

            scheduleControl1.Appearance.VisualStyle = GridVisualStyles.Office2007Blue;
            scheduleControl1.Appearance.WorkWeekHeaderForeColor = Color.White;
            scheduleControl1.Appearance.WorkWeekHeaderBackColor = Color.FromArgb(200,224,255);
            
            scheduleControl1.Appearance.WeekCalendarStartDayOfWeek =
                CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            
            if (toolStripComboBoxScenario.ComboBox != null)
            {
                toolStripComboBoxScenario.ComboBox.DisplayMember = "Description";
                toolStripComboBoxScenario.ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                toolStripComboBoxScenario.ComboBox.DataSource = _dataProvider.AllowedScenarios();
                
                if (_dataProvider.Scenario != null)
                    toolStripComboBoxScenario.ComboBox.SelectedItem = _dataProvider.Scenario;

                toolStripComboBoxScenario.ComboBox.SelectedIndexChanged += comboBoxScenarioSelectedIndexChanged;
            }

            _calendarAndTextPanel.DateChanged += calendarAndTextPanelDateChanged;
            _nextButton.Click += nextButtonClick;
            _previousButton.Click += previousButtonClick;
            scheduleControl1.NavigationPanel.Resize += navigationPanelResize;
        	scheduleControl1.Appearance.ScheduleAppointmentTipFormat = "[subject]\r\n[content]\r\n\r\n[marker]";
            scheduleControl1.DataSource = _dataProvider;
        	
        }

        

        private void meetingOverviewViewShown(object sender, EventArgs e)
        {
            var scenarios = _dataProvider.AllowedScenarios();
            if (scenarios.Count == 0)
            {
                ShowErrorMessage(Resources.NoAllowedScenarios);
                Close();
                return;
            }
            scheduleControl1.Calendar.DateValue = DateTime.Now;
            scheduleControl1.ScheduleType = ScheduleViewType.WorkWeek;

            scheduleControl1.PerformSwitchToScheduleViewTypeClick(ScheduleViewType.WorkWeek);
            
            if (!TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings))
            {
                scheduleControl1.AllowAdjustAppointmentsWithMouse = false;
            }

            
           scheduleControl1.Calendar.Visible = false;
            _calendarAndTextPanel.BringToFront();
           scheduleControl1.Culture = CultureInfo.CurrentCulture;
            scheduleControl1.Appearance.WorkWeekHeaderFormat = "d dddd";
           selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
           if (scheduleControl1.RightToLeft == RightToLeft.Yes)
               scheduleControl1.NavigationPanelPosition = CalendarNavigationPanelPosition.Right;
           _nextButton.Visible = true;
           _previousButton.Visible = true;
            scheduleControl1.Visible = true;
			ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlAdv1BeforeContextMenuOpen;
        }

    	static void ribbonControlAdv1BeforeContextMenuOpen(object sender, Syncfusion.Windows.Forms.Tools.ContextMenuEventArgs e)
		{
			e.Cancel = true;
		}

        private void meetingOverviewViewFormClosed(object sender, FormClosedEventArgs e)
        {
            scheduleControl1.RemoveControlFromNavigationPanel(_calendarAndTextPanel);
            _calendarAndTextPanel.Dispose();
            
            if (toolStripComboBoxScenario.ComboBox != null)
                toolStripComboBoxScenario.ComboBox.SelectedIndexChanged += comboBoxScenarioSelectedIndexChanged;

            scheduleControl1.NavigationPanel.Resize -= navigationPanelResize;
            _calendarAndTextPanel.DateChanged -= calendarAndTextPanelDateChanged;
            
            _nextButton.Click -= nextButtonClick;
            _previousButton.Click -= previousButtonClick;
            _eventAggregator.GetEvent<PersonSelectionFormHideEvent>().Unsubscribe(unCheckFilter);
            _meetingOverviewFilter.Close();
        }

        void comboBoxScenarioSelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxScenario.ComboBox == null) return;
            _dataProvider.Scenario = (IScenario)toolStripComboBoxScenario.ComboBox.SelectedItem;
            //maybe own event??????
            _eventAggregator.GetEvent<MeetingModificationOccurred>().Publish(string.Empty);
        }

        void scheduleControl1SetupContextMenu(object sender, System.ComponentModel.CancelEventArgs e)
        {
            scheduleControl1.GetScheduleHost().ContextMenu = new ContextMenu();
            e.Cancel = true;
        }

        void previousButtonClick(object sender, EventArgs e)
        {
            moveDays(-7);
        }

        void nextButtonClick(object sender, EventArgs e)
        {
            moveDays(7);
        }

        private void moveDays(int days)
        {
            _calendarAndTextPanel.SelectedDate = scheduleControl1.RightToLeft == RightToLeft.Yes ? _calendarAndTextPanel.SelectedDate.AddDays(-days) : _calendarAndTextPanel.SelectedDate.AddDays(days);
        }

        void scheduleControl1ScheduleAppointmentClick(object sender, ScheduleAppointmentClickEventArgs e)
        {
            _clickedDate = e.ClickDateTime;
            _selectedItem = e.Item;
            _eventAggregator.GetEvent<AppointmentSelectionChanged>().Publish(string.Empty);

            if(e.ClickType == ScheduleAppointmentClickType.LeftDblClick)
                if(_selectedItem != null)
                    _eventAggregator.GetEvent<EditAppointmentClicked>().Publish(string.Empty);
                else               
                    _eventAggregator.GetEvent<AddAppointmentClicked>().Publish(string.Empty);

            if (e.ClickType == ScheduleAppointmentClickType.RightClick)
            {
                contextMenuStripSchedule.Show(Cursor.Position);

            }
        }

        public bool ConfirmDeletion(IMeeting theMeeting)
        {
            if (theMeeting == null)
                return false;
            DialogResult answer = MessageBox.Show(string.Format(CultureInfo.InvariantCulture, Resources.MeetingDeleteConfirmationQuestion, theMeeting.GetSubject(new NoFormatting())), Resources.ConfirmDelete, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
           if(answer == DialogResult.No)
                return false;

           return true;
        }

        public void ReloadMeetings()
        {
            _dataProvider.ResetLoadedPeriod();
            selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
        }

        void ScheduleControl1ShowingAppointmentForm(object sender, ShowingAppointFormEventArgs e)
        {
            e.Cancel = true;
        }

        void scheduleControl1ItemChanging(object sender, ScheduleAppointmentCancelEventArgs e)
        {
            // call save
            if (e.Action == ItemAction.ItemDrag || e.Action == ItemAction.TimeDrag)
            {
                var propItem = e.ProposedItem;
                var currItem = e.CurrentItem;
                if(propItem.StartTime.Equals(currItem.StartTime) && propItem.EndTime.Equals(currItem.EndTime))
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    _dataProvider.SaveAppointment(e.ProposedItem, e.CurrentItem, this);
                }
                catch (OptimisticLockException)
                {
                    ShowErrorMessage(Resources.SomeoneChangedTheSameDataBeforeYouDot);
                    _eventAggregator.GetEvent<MeetingModificationOccurred>().Publish(string.Empty);
                    return;
                }
                catch (DataSourceException dataSourceException)
                {
                    ShowDataSourceException(dataSourceException);
                    
                    return;
                }
                selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
                return;
           }
            
            e.Cancel = true;
        }

        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, Resources.ErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
        }

        public DateOnlyPeriod SelectedWeek
        {
            get
            {
                var dates = scheduleControl1.Calendar.SelectedDates;
                var start = new DateOnly((DateTime) dates.ToArray().First());
                var end = new DateOnly((DateTime) dates.ToArray().Last());
                return new DateOnlyPeriod(start,end);
            }
        }

        public void ShowDataSourceException(DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.MeetingOverview,
                                                                    Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

    	public bool FetchForCurrentUser
    	{
			get { return toolStripButtonFetchForUser.Checked; }
    	}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        private void selectWholeWeekInCalendar(DateTime selectedDate)
        {
            _selectedItem = null;
            DateTime firstDay = selectedDate;
            // use used culture (UI or not?????)
            
            var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            while (firstDay.DayOfWeek != firstDayOfWeek)
                firstDay = firstDay.AddDays(-1);
            scheduleControl1.Calendar.SelectedDates.BeginUpdate();
            scheduleControl1.Calendar.SelectedDates.Clear();
            var dates = new[]
                            {
                                firstDay, firstDay.AddDays(1), firstDay.AddDays(2), firstDay.AddDays(3),
                                firstDay.AddDays(4), firstDay.AddDays(5), firstDay.AddDays(6)
                            };
            scheduleControl1.Calendar.SelectedDates.AddRange(dates);
            scheduleControl1.Calendar.SelectedDates.EndUpdate();

            scheduleControl1.Calendar.SetDateValue(selectedDate);
            _calendarAndTextPanel.SelectDateRange(scheduleControl1.Calendar.SelectedDates);

            DateTime dt = (scheduleControl1.Calendar.SelectedDates.Count > 0) ? scheduleControl1.Calendar.SelectedDates[0] : scheduleControl1.Calendar.DateValue;
            scheduleControl1.HeaderLabel.Text = string.Concat(Resources.Starting, " ", dt.ToLongDateString());
            _eventAggregator.GetEvent<AppointmentSelectionChanged>().Publish(string.Empty);
        }

        void calendarAndTextPanelDateChanged(object sender, EventArgs e)
        {
            selectWholeWeekInCalendar(_calendarAndTextPanel.SelectedDate);
        }

        private void resizeCalendarToNavigationPanel()
        {
            _calendarAndTextPanel.Left = 0;
            _calendarAndTextPanel.Top = 0;
            _calendarAndTextPanel.Height = scheduleControl1.NavigationPanel.Height;
            _calendarAndTextPanel.Width = scheduleControl1.NavigationPanel.Width;
            // funkar inte, kontrollen flyttar tillbaka dom
            if (scheduleControl1.RightToLeft == RightToLeft.Yes)
            {
                _previousButton.Left = 0;
                _nextButton.Left = scheduleControl1.NavigationPanel.Left;
            }
        }

        void navigationPanelResize(object sender, EventArgs e)
        {
            resizeCalendarToNavigationPanel();
        }

        public DateTime SelectedDate
        {
             get { return _calendarAndTextPanel.SelectedDate; } 
        }

        public IMeeting SelectedMeeting
        {
            get
            {
                if (_selectedItem != null)
                    return _selectedItem.Tag as IMeeting;
                return null;
            }
        }

        public ICccTimeZoneInfo UserTimeZone { get; set; }

        private void deleteClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<DeleteAppointmentClicked>().Publish(string.Empty);
        }

        private void addClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<AddAppointmentClicked>().Publish(string.Empty);
        }

        private void openClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<EditAppointmentClicked>().Publish(string.Empty);
        }

        private void cutClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<CutAppointmentClicked>().Publish(string.Empty);
        }

        private void copyClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<CopyAppointmentClicked>().Publish(string.Empty);
        }

        private void pasteClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<PasteAppointmentClicked>().Publish(string.Empty);
        }

        private void toolStripButtonExportClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<ShowExportMeetingClicked>().Publish(string.Empty);
        }

        private void toolStripButtonCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void EditMeeting(IMeetingViewModel meetingViewModel)
        {
            var viewSchedulesPermission = isPermittedToViewSchedules();
            var meetingComposerView = new MeetingComposerView(meetingViewModel, null, true, viewSchedulesPermission,
                                                              _eventAggregator);
            meetingComposerView.Show(this);
        }

        public bool EditEnabled
        {
            get { return toolStripMenuItemOpen.Enabled; }
            set 
            {
                toolStripMenuItemOpen.Enabled = value;
                toolStripButtonOpen.Enabled = value;
            }
        }

        public bool DeleteEnabled
        {
            get { return toolStripMenuItemDelete.Enabled; }
            set
            {
                toolStripMenuItemDelete.Enabled = value;
                toolStripButtonDelete.Enabled = value;
            }
        }

        public bool CopyEnabled
        {
            get { return toolStripButtonCopy.Enabled; }
            set
            {
                toolStripMenuItemCopy.Enabled = value;
                toolStripButtonCopy.Enabled = value;
            }
        }

        public bool PasteEnabled
        {
            get { return toolStripButtonPaste.Enabled; }
            set
            {
                toolStripMenuItemPaste.Enabled = value;
                toolStripButtonPaste.Enabled = value;
            }
        }

        public bool CutEnabled
        {
            get { return toolStripButtonCut.Enabled; }
            set
            {
                toolStripMenuItemCut.Enabled = value;
                toolStripButtonCut.Enabled = value;
            }
        }

        public bool AddEnabled
        {
            get { return toolStripMenuItemNew.Enabled; }
            set
            {
                toolStripMenuItemNew.Enabled = value;
                toolStripButtonAdd.Enabled = value;
            }
        }

        public bool ExportEnabled
        {
            get { return toolStripButtonExport.Enabled; }
            set
            {
                toolStripButtonExport.Enabled = value;
            }
        }

        public MeetingOverviewPresenter Presenter{ get; set; }
        
        public void SetInfoText(string text)
        {
            _calendarAndTextPanel.SetText(text);
        }

        private static bool isPermittedToViewSchedules()
        {
            IPerson person = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
            ITeam rightClickedPersonsTeam = person.MyTeam(DateOnly.Today);
            if (TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules, DateOnly.Today, rightClickedPersonsTeam))
            {
                return true;
            }
            return false;

        }

        public DateTimePeriod SelectedPeriod()
        {
            DateTime startDateTime;
            DateTime endDateTime;
            getStartAndEndTimes(out startDateTime, out endDateTime);
            startDateTime = DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc);
            endDateTime = DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc);
            return new DateTimePeriod(startDateTime, endDateTime);
        }

        private void getStartAndEndTimes(out DateTime startDateTime, out DateTime endDateTime)
        {
            GridModelSelections gridModelSelections = scheduleControl1.GetScheduleHost().Model.Selections;
            GridRangeInfoList selectedRowRange = gridModelSelections.GetSelectedRows(false, true);
            int divisionPerHour = scheduleControl1.Appearance.DivisionsPerHour;
            startDateTime = DateTime.MinValue;
            endDateTime = DateTime.MinValue;
            //for multiple or single selection
            if (selectedRowRange.Count > 0)
            {
                const int rowOffset = 2;
                if (selectedRowRange[0].Top == 1) //This is the top Blue box 
                {
                    getAllDayDateTimes(out startDateTime, out endDateTime);
                }
                else
                {
                    int timeStartRow = selectedRowRange[0].Top - rowOffset;
                    int timeEndRow = selectedRowRange[0].Bottom - rowOffset;
                    int totalMinutes = ((timeEndRow - timeStartRow) + 1) * (60 / divisionPerHour);
                    int startHour = timeStartRow / divisionPerHour;
                    int startMinute = (timeStartRow % divisionPerHour) * (60 / divisionPerHour);

                    int col = 0;
                    var selCol = gridModelSelections.GetSelectedCols(true, true);
                    if (selCol[0].Left > 3)
                        col = selCol[0].Left / 31;

                    if(scheduleControl1.Calendar.SelectedDates.Count -1 >= col)
                        _clickedDate = scheduleControl1.Calendar.SelectedDates[col];

                    startDateTime = _clickedDate.Date.AddHours(startHour).AddMinutes(startMinute);
                    endDateTime = startDateTime.AddMinutes(totalMinutes);
                }
            }
        }

        private void getAllDayDateTimes(out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime =_clickedDate.Date;
            endDateTime = _clickedDate.Date.AddDays(1).AddMinutes(-1);
        }

        
        private void toolStripButtonFilterClick(object sender, EventArgs e)
        {
            toolStripButtonFilter.Checked = true;
            Point pointToScreen = toolStripExFilter.PointToScreen(new Point(toolStripButtonFilter.Bounds.X, toolStripButtonFilter.Bounds.Y + toolStripButtonFilter.Height));
            _meetingOverviewFilter.Location = pointToScreen;
            _meetingOverviewFilter.Show();
            _meetingOverviewFilter.Visible = true;
        }

        private void unCheckFilter(string something)
        {
            toolStripButtonFilter.Checked = false;
        }

		private void toolStripButtonFetchForUserClick(object sender, EventArgs e)
		{
			_eventAggregator.GetEvent<FetchForCurrentUserChanged>().Publish(toolStripButtonFetchForUser.Checked);
		}

    }
}
