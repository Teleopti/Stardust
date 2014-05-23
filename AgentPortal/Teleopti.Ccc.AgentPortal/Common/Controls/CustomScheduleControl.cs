using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public class CustomScheduleControl : ScheduleControl
    {
        private ICustomScheduleAppointment _clickedScheduleAppointment;
        private bool _isTimelineSelected;
        private readonly Collection<ICustomScheduleAppointment> _selectedScheduleAppointments = new Collection<ICustomScheduleAppointment>();
	    private ScheduleViewType _previousView;
        private Control _previousButton; 
        private Control _nextButton;
        private bool _isHeaderSelected;

        public bool IsHeaderSelected
        {
            get { return _isHeaderSelected; }
        }

        public ICustomScheduleAppointment ClickedScheduleAppointment
        {
            get { return _clickedScheduleAppointment; }
            set { _clickedScheduleAppointment = value;}
        }

        public Collection<ICustomScheduleAppointment> SelectedScheduleAppointments
        {
            get { return _selectedScheduleAppointments; }
        }

        public IList<DateTime> SelectedDates
        {
            get
            {
                ScheduleGrid scheduleGrid = GetScheduleHost();
                CustomScheduleGrid customScheduleGrid = (CustomScheduleGrid) scheduleGrid;
                return customScheduleGrid.Dates;
            }
        }

        public bool IsTimelineSelected
        {
            get { return _isTimelineSelected; }
        }

	    public DateTime ClickedDate { get; set; }

	    public event EventHandler<EventArgs> ScheduleTypeChanged;

        public event EventHandler<CustomScheduleAppointmentDeleteEventArgs> DeleteScheduleAppointment;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public override ScheduleGrid CreateScheduleGrid(NavigationCalendar calendar, ScheduleControl schedule, DateTime initialDate)
        {
			calendar.CalenderGrid.QueryCellInfo += CalendarQueryCellInfo;
			
            CustomScheduleGrid scheduleGrid = new CustomScheduleGrid(calendar, schedule, initialDate);
            scheduleGrid.MouseDown += ScheduleGridMouseDown;
            scheduleGrid.KeyDown += ScheduleGrid_KeyDown;
            scheduleGrid.AppointmentsCopy += ScheduleGridAppointmentsCopy;
            scheduleGrid.AppointmentsPaste += ScheduleGridAppointmentsPaste;
            scheduleGrid.GridVisualStyles = GridVisualStyles.Office2007Blue;
            scheduleGrid.ThemesEnabled = true;
            return scheduleGrid;
        }

		private void CalendarQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
	    {
			const int rowsPerCal = 8;
			int rest = e.RowIndex % rowsPerCal;
			if ((e.ColIndex > 0) && (rest == 1))
			{
				var weekdays = DateHelper.GetDaysOfWeek(Culture);
				e.Style.CellValue = Culture.DateTimeFormat.GetShortestDayName(weekdays[e.ColIndex-1])[0];
			}
	    }
        public override void OnScheduleAppointmentClick(ScheduleAppointmentClickEventArgs e)
        {
            CustomScheduleGrid scheduleGrid = (CustomScheduleGrid)GetScheduleHost();
            //Store Clicked dates
            ClickedDate = e.ClickDateTime.Date;
            if (e.ClickType == ScheduleAppointmentClickType.LeftClick)
            {
                scheduleGrid.AddSelectedDate(e.ClickDateTime.Date);
            }

            _clickedScheduleAppointment = (ICustomScheduleAppointment)e.Item;

            //items to copy/paste
            if (_clickedScheduleAppointment != null && _clickedScheduleAppointment.AllowCopy)
            {
                if (scheduleGrid.ModifierKeyIsPressed)
                {
                    if (!_selectedScheduleAppointments.Contains(_clickedScheduleAppointment))
                        _selectedScheduleAppointments.Add(_clickedScheduleAppointment);
                }
                else
                {
                    _selectedScheduleAppointments.Clear();
                    _selectedScheduleAppointments.Add(_clickedScheduleAppointment);
                }
            }
           
            base.OnScheduleAppointmentClick(e);
        }

        internal void OnScheduleTypeChange()
        {
            if (ScheduleTypeChanged != null)
            {
                if (_previousView != ScheduleType)
                {
                    if ((ScheduleType == ScheduleViewType.Week))
                    {
                        _previousView = ScheduleViewType.CustomWeek;
                        SetSevenDayWeek();
                    }

                    ScheduleTypeChanged.Invoke(this,new EventArgs());
                    _previousView = ScheduleType;
                }
                else if (ScheduleType == ScheduleViewType.CustomWeek)
                {
                    _previousButton.Visible = true;
                    _nextButton.Visible = true;
                }
            }
        }

        internal void SetSevenDayWeek()
        {
            Appearance.WeekHeaderFormat = "MMMM dd";
            DateSelections dates = Calendar.SelectedDates;
            DateTime firstDay = dates[0];

            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            while (firstDay.DayOfWeek != firstDayOfWeek)
            {
                firstDay = firstDay.AddDays(-1);
            }
            Calendar.SelectedDates.BeginUpdate();
            Calendar.SelectedDates.Clear();
            Calendar.SelectedDates.AddRange(
                new[]{firstDay, firstDay.AddDays(1), firstDay.AddDays(2), firstDay.AddDays(3),
                firstDay.AddDays(4), firstDay.AddDays(5), firstDay.AddDays(6)});
            Calendar.SelectedDates.EndUpdate(true);

            GetScheduleHost().SwitchTo(ScheduleViewType.CustomWeek, true);
			Appearance.ScheduleAppointmentTipsEnabled = true;
            _previousButton.Visible = true;
            _nextButton.Visible = true;
        }

        public CustomScheduleControl()
        {
            InitializeComponent();
            Initialize();
        }

        internal void ScheduleGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if ((_clickedScheduleAppointment != null) && (DeleteScheduleAppointment != null))
                {
                    //filtering
                    if (_clickedScheduleAppointment.AppointmentType == ScheduleAppointmentTypes.Request)
                    {
                        DeleteScheduleAppointment.Invoke(this,
                                                  new CustomScheduleAppointmentDeleteEventArgs(
                                                      ));
                    }
                }
            }
        }

        private void ScheduleGridMouseDown(object sender, MouseEventArgs e)
        {
            int rowIndex;
            int colIndex;
            GetScheduleHost().PointToRowCol(e.Location, out rowIndex, out colIndex);
            if (ScheduleType == ScheduleViewType.Month)
                _isTimelineSelected = false;
            else
                _isTimelineSelected = colIndex == 1 ? true : false;
            _isHeaderSelected = rowIndex == 0 ? true : false;
        }

        private void ScheduleGridAppointmentsPaste(object sender, EventArgs e)
        {
            CustomScheduleAppointmentCopyPasteEventArgs arg = new CustomScheduleAppointmentCopyPasteEventArgs();
            OnPasteScheduleAppointment(arg);

        }

        private void ScheduleGridAppointmentsCopy(object sender, EventArgs e)
        {
            CustomScheduleAppointmentCopyPasteEventArgs arg = new CustomScheduleAppointmentCopyPasteEventArgs();
            OnCopyScheduleAppointment(arg);
        }

        private void NextButtonClick(object sender, EventArgs e)
        {
           
            if (ScheduleType == ScheduleViewType.CustomWeek)
            {
                Control captionButton = sender as Control;
                NavigateInWeekView(captionButton);
            }
            else if (ScheduleType == ScheduleViewType.Month)
            {
                //Seems like the Syncfusion behavior has changed. Removed code to step 1 month forward and everything works anyway. 
                //If I remove code to step 1 month backward it does not work. Strange!!
                //Next syncfusion version this might break again. Ola

               // Calendar.AdjustSelectionsByMonth(1);
            }
        }

        private void PreviousButtonClick(object sender, EventArgs e)
        {
            if (ScheduleType == ScheduleViewType.CustomWeek)
            {
                Control captionButton = sender as Control;
                NavigateInWeekView(captionButton);
            }
            else if (ScheduleType == ScheduleViewType.Month)
            {
                Calendar.AdjustSelectionsByMonth(-1);
            }
        }

        private void Initialize()
        {
            _previousView = ScheduleType;
            Appearance.WeekMonthNewMonth = "d"; // Must have full date for parsing later on.
            Appearance.WeekMonthItemFormat = "[starttime] - [endtime]\n[subject]";
            Appearance.AllDayItemFormat = "[subject]";
            Appearance.DayItemFormat = "[subject] [starttime] - [endtime]\n[location]";
            Appearance.ScheduleAppointmentTipsEnabled = true;
            CaptionPanel.ContextMenu = new ContextMenu(); //Block right click
            _nextButton = CaptionPanel.Controls[1];
            _previousButton = CaptionPanel.Controls[2];
            _nextButton.ContextMenu = new ContextMenu();
            _previousButton.ContextMenu = new ContextMenu();
            _previousButton.Click += PreviousButtonClick;
            _nextButton.Click += NextButtonClick;

			// <-- Bugfix #27980: Strange view in Week view when logging in to MyTime
			Calendar.Resize += calendar_Resize;
			// -->
        }

		void calendar_Resize(object sender, EventArgs e)
		{
			Timer latency = new Timer {Interval = 500};
			latency.Tick += latency_Tick;
			latency.Start();
		}

		void latency_Tick(object sender, EventArgs e)
		{
			var timer = (Timer)sender;
			timer.Stop();
			timer.Dispose();
			OnResize(e);
		}

        private void NavigateInWeekView(Control captionButton)
        {
            int dayCount = Calendar.SelectedDates.Count;
            DateTime[] navigatedDays = new DateTime[dayCount];
            dayCount = (captionButton == _previousButton) ? (dayCount * -1) : dayCount;

            for (int t = 0; t < navigatedDays.Length; t++)
            {
                navigatedDays[t] = Calendar.SelectedDates[t].AddDays(dayCount);
            }

            Calendar.SelectedDates.BeginUpdate();
            Calendar.SelectedDates.Clear();
            Calendar.SelectedDates.AddRange(navigatedDays);
            Calendar.SelectedDates.EndUpdate(true);

            GetScheduleHost().SwitchTo(ScheduleViewType.CustomWeek, true);
            _nextButton.Visible = true;
            _previousButton.Visible = true;
            GetScheduleHost().SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
        }

        protected virtual void OnCopyScheduleAppointment(CustomScheduleAppointmentCopyPasteEventArgs arg)
        {
        }

        protected virtual void OnPasteScheduleAppointment(CustomScheduleAppointmentCopyPasteEventArgs arg)
        {
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Name = "CustomScheduleControl";
            ResumeLayout(false);
        }
    }
}
