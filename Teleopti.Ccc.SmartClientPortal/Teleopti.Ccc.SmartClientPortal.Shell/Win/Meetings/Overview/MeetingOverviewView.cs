using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview
{
	public partial class MeetingOverviewView : BaseRibbonForm, IMeetingOverviewView
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly CalendarAndTextPanel _calendarAndTextPanel;
		private readonly MeetingsScheduleProvider _dataProvider;
		private readonly IMeetingOverviewFilter _meetingOverviewFilter;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IScheduleStorageFactory _scheduleStorageFactory;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly ITimeZoneGuard _timeZoneGuard;

		private IScheduleAppointment _selectedItem;
		private readonly Control _nextButton;
		private readonly Control _previousButton;
		private DateTime _clickedDate;


		public MeetingOverviewView(IEventAggregator eventAggregator, MeetingsScheduleProvider dataProvider,IMeetingOverviewFilter meetingOverviewFilter,
									IResourceCalculation resourceOptimizationHelper, IScheduleStorageFactory scheduleStorageFactory, 
									CascadingResourceCalculationContextFactory resourceCalculationContextFactory, ITimeZoneGuard timeZoneGuard)
		{
			_eventAggregator = eventAggregator;
			_dataProvider = dataProvider;
			_meetingOverviewFilter = meetingOverviewFilter;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleStorageFactory = scheduleStorageFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_timeZoneGuard = timeZoneGuard;

			InitializeComponent();
			SetTexts();
			_eventAggregator.GetEvent<PersonSelectionFormHideEvent>().Subscribe(unCheckFilter);

			UserTimeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
			ScheduleGrid.DisplayStrings[4] = "";//hide the text area in all day area in day view
			ScheduleGrid.DisplayStrings[3] = "";
			scheduleControl1.Appearance.WorkWeekHeaderFormat = "d dddd";
			// for bug 29606
			scheduleControl1.ScheduleType = ScheduleViewType.WorkWeek;

			scheduleControl1.Visible = false;
			// to set visible in CustomWeek
			_nextButton = scheduleControl1.CaptionPanel.Controls[1];
			_previousButton = scheduleControl1.CaptionPanel.Controls[2];
			_dataProvider.CreateListObjectList();

			_calendarAndTextPanel = new CalendarAndTextPanel();
			scheduleControl1.AddControlToNavigationPanel(_calendarAndTextPanel);
			resizeCalendarToNavigationPanel();

			scheduleControl1.Appearance.VisualStyle = GridVisualStyles.Office2007Blue;
			scheduleControl1.Appearance.WeekCalendarStartDayOfWeek =
				CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
			
			if (toolStripComboBoxScenario.ComboBox != null)
			{
				toolStripComboBoxScenario.ComboBox.DisplayMember = "Description";
				toolStripComboBoxScenario.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				toolStripComboBoxScenario.ComboBox.DataSource = _dataProvider.AllowedScenarios();
				
				if (_dataProvider.Scenario != null)
					toolStripComboBoxScenario.ComboBox.SelectedItem = _dataProvider.Scenario;

				toolStripComboBoxScenario.ComboBox.SelectedIndexChanged += comboBoxScenarioSelectedIndexChanged;
			}
			if (toolStripComboBoxResolution.ComboBox != null)
			{
				toolStripComboBoxResolution.ComboBox.DisplayMember = "Resolution";
				toolStripComboBoxResolution.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				toolStripComboBoxResolution.ComboBox.Items.AddRange(GridResolution.GetCollection().Cast<object>().ToArray());
				toolStripComboBoxResolution.ComboBox.SelectedIndex = 1;
				toolStripComboBoxResolution.ComboBox.SelectedIndexChanged += comboBoxSelectedIndexChanged;
			}
			_calendarAndTextPanel.DateChanged += calendarAndTextPanelDateChanged;
			_nextButton.Click += nextButtonClick;
			_previousButton.Click += previousButtonClick;
			scheduleControl1.NavigationPanel.Resize += navigationPanelResize;
			scheduleControl1.Appearance.ScheduleAppointmentTipFormat = "[subject]\r\n[content]\r\n\r\n[marker]";
			scheduleControl1.DataSource = _dataProvider;
			Padding = new Padding(2);
		}

		void comboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			var item = toolStripComboBoxResolution.ComboBox.SelectedItem as GridResolution;
			if (item != null)
				scheduleControl1.Appearance.DivisionsPerHour = item.PerHour;
			selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
			_previousButton.Visible = true;
			_nextButton.Visible = true;
			scheduleControl1.Appearance.CaptionBackColor = Color.White;
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
			
			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings))
			{
				scheduleControl1.AllowAdjustAppointmentsWithMouse = false;
			}
			//using a lot of cpu and memory, don't know why
			// don't use it for now
			//scheduleControl1.GetScheduleHost().Schedule.Appearance.VisualStyle = GridVisualStyles.Metro;

			scheduleControl1.NavigationPanel.Width = 220;
		   scheduleControl1.Calendar.Visible = false;
			_calendarAndTextPanel.BringToFront();
           scheduleControl1.Culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
		   scheduleControl1.Appearance.WorkWeekHeaderFormat = "d dddd";
		   selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
		   if (scheduleControl1.RightToLeft == RightToLeft.Yes)
			   scheduleControl1.NavigationPanelPosition = CalendarNavigationPanelPosition.Right;
		   _nextButton.Visible = true;
		   _previousButton.Visible = true;
			scheduleControl1.Visible = true;
			ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlAdv1BeforeContextMenuOpen;
			
			scheduleControl1.Appearance.CaptionBackColor = Color.White;
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
			DialogResult answer = ShowYesNoMessage(string.Format(CultureInfo.CurrentCulture, Resources.MeetingDeleteConfirmationQuestion, theMeeting.GetSubject(new NoFormatting())), Resources.ConfirmDelete);
		   if(answer == DialogResult.No)
				return false;

		   return true;
		}

		public void ReloadMeetings()
		{
			_dataProvider.ResetLoadedPeriod();
			selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
		}

		void scheduleControl1ShowingAppointmentForm(object sender, ShowingAppointFormEventArgs e)
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
			MessageBoxAdv.Show(message, Resources.ErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
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
			if (_calendarAndTextPanel == null)
				return;
			_selectedItem = null;
			DateTime firstDay = selectedDate.Date;
			
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
			scheduleControl1.HeaderLabel.Text = string.Concat(Resources.Starting, " ", dt.ToShortDateString());
			_eventAggregator.GetEvent<AppointmentSelectionChanged>().Publish(string.Empty);
			Refresh();
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
			Refresh();
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

		public TimeZoneInfo UserTimeZone { get; set; }

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
		public void EditMeeting(IMeetingViewModel meetingViewModel, IIntraIntervalFinderService intraIntervalFinderService, ISkillPriorityProvider skillPriorityProvider, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			var viewSchedulesPermission = isPermittedToViewSchedules();
			var meetingComposerView = new MeetingComposerView(meetingViewModel, null, true, viewSchedulesPermission,
															  _eventAggregator, _resourceOptimizationHelper, skillPriorityProvider,
															  _scheduleStorageFactory,
															  staffingCalculatorServiceFacade,
															  _resourceCalculationContextFactory,
																_timeZoneGuard);
			meetingComposerView.ShowDialog(this);
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
			IPerson person = ((ITeleoptiPrincipalWithUnsafePerson)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal).UnsafePerson();
			ITeam rightClickedPersonsTeam = person.MyTeam(DateOnly.Today);
			if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules, DateOnly.Today, rightClickedPersonsTeam))
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
			if (scheduleControl1 == null)
				MessageBox.Show("scheduleControl1 == null");
			if(scheduleControl1.GetScheduleHost() == null)
				MessageBox.Show("GetScheduleHost() == null");
			if(scheduleControl1.GetScheduleHost().Model == null)
				MessageBox.Show("Model == null");
			
			GridModelSelections gridModelSelections = scheduleControl1.GetScheduleHost().Model.Selections;
			if (gridModelSelections == null)
				MessageBox.Show("gridModelSelections == null");

			GridRangeInfoList selectedRowRange = gridModelSelections.GetSelectedRows(false, true);
			if (selectedRowRange == null)
				MessageBox.Show("selectedRowRange == null");

			if (scheduleControl1.Appearance == null)
				MessageBox.Show("scheduleControl1.Appearance == null");
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
					if (selCol == null)
						MessageBox.Show("selCol == null");

					if (scheduleControl1.Calendar == null)
						MessageBox.Show("selCol.Count == 0");
					if (selCol[0].Left > 3)
						col = selCol[0].Left / 31;

					if (scheduleControl1.Calendar == null)
						MessageBox.Show("scheduleControl1.Calendar == null");
					if (scheduleControl1.Calendar.SelectedDates == null)
						MessageBox.Show("Calendar.SelectedDates == null");

					if(scheduleControl1.Calendar.SelectedDates.Count -1 > col) //<- var lika med här
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

		internal class GridResolution
		{
			public static IList<GridResolution> GetCollection()
			{
				return new List<GridResolution>
					{
						new GridResolution {PerHour = 1, Resolution = 60},
						new GridResolution {PerHour = 2, Resolution = 30},
						new GridResolution {PerHour = 4, Resolution = 15},
						new GridResolution {PerHour = 6, Resolution = 10},
						new GridResolution {PerHour = 12, Resolution = 5}
					};
			}
			public int PerHour { get; set; }
			public int Resolution { get; set; }
		}

		private void scheduleControl1SizeChanged(object sender, EventArgs e)
		{
			_dataProvider.ResetLoadedPeriod();
			selectWholeWeekInCalendar(scheduleControl1.Calendar.DateValue);
		}
	}
}
