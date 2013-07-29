using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using System.Globalization;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    /// <summary>
    /// Represent the exended shceudle control with custom functionalites
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-11-20
    /// </remarks>
    public class CustomScheduleControl : ScheduleControl
    {
        private ICustomScheduleAppointment _clickedScheduleAppointment;
        private bool _isTimelineSelected;
        private readonly Collection<ICustomScheduleAppointment> _selectedScheduleAppointments = new Collection<ICustomScheduleAppointment>();
        private DateTime _clickedDate;
        private ScheduleViewType _previousView;
        private Control _previousButton; 
        private Control _nextButton;
        private bool _isHeaderSelected;

        public bool IsHeaderSelected
        {
            get { return _isHeaderSelected; }
        }

        /// <summary>
        /// Gets the schedule appointment.
        /// </summary>
        /// <value>The schedule appointment.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/28/2008
        /// </remarks>
        public ICustomScheduleAppointment ClickedScheduleAppointment
        {
            get { return _clickedScheduleAppointment; }
            set { _clickedScheduleAppointment = value;}
        }

        /// <summary>
        /// Gets the selected schedule appointments.
        /// </summary>
        /// <value>The selected schedule appointments.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-05
        /// </remarks>
        public Collection<ICustomScheduleAppointment> SelectedScheduleAppointments
        {
            get { return _selectedScheduleAppointments; }
        }

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <value>The dates.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-10
        /// </remarks>
        public IList<DateTime> SelectedDates
        {
            get
            {
                ScheduleGrid scheduleGrid = GetScheduleHost();
                CustomScheduleGrid customScheduleGrid = (CustomScheduleGrid) scheduleGrid;
                return customScheduleGrid.Dates;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is time line selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is time line selected; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-20
        /// </remarks>
        public bool IsTimelineSelected
        {
            get { return _isTimelineSelected; }
        }

        /// <summary>
        /// Gets or sets the clicked date.
        /// </summary>
        /// <value>The clicked date.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        public DateTime ClickedDate
        {
            get { return _clickedDate; }
            set { _clickedDate = value; }
        }

        /// <summary>
        /// Raise after View type of schedule control has changed
        /// </summary>
        public event EventHandler<EventArgs> ScheduleTypeChanged;

        /// <summary>
        /// Occurs when [delete schedule item].
        /// </summary>
        /// <remarks>
        /// Created by: Sachinthaw
        /// Created date: 2008-11-18
        /// </remarks>
        public event EventHandler<CustomScheduleAppointmentDeleteEventArgs> DeleteScheduleAppointment;

        /// <summary>
        /// Occurs when [copy schedule appointment].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        public event EventHandler<CustomScheduleAppointmentCopyPasteEventArgs> CopyScheduleAppointment;

        /// <summary>
        /// Occurs when [paste schedule appointment].
        /// </summary>
        /// <remarks>
        /// Created by: Sachinthaw
        /// Created date: 2009-01-16
        /// </remarks>
        public event EventHandler<CustomScheduleAppointmentCopyPasteEventArgs> PasteScheduleAppointment;

        /// <summary>
        /// Creates the ScheduleGrid used by this ScheduleControl.
        /// </summary>
        /// <param name="calendar">The NavigationCalendar to be used by this ScheduleControl.</param>
        /// <param name="schedule">The ScheduleControl that is the parent of this ScheduleGrid.</param>
        /// <param name="initialDate"></param>
        /// <returns>
        /// ScheduleGrid used by this ScheduleControl.
        /// </returns>
        /// <remarks>
        /// Override this method if you want your ScheduleControl to use a derived ScheduleGrid.
        /// </remarks>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-20
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public override ScheduleGrid CreateScheduleGrid(NavigationCalendar calendar, ScheduleControl schedule, DateTime initialDate)
        {
            CustomScheduleGrid scheduleGrid = new CustomScheduleGrid(calendar, schedule, initialDate);
            scheduleGrid.MouseDown += ScheduleGridMouseDown;
            scheduleGrid.KeyDown += ScheduleGrid_KeyDown;
            scheduleGrid.AppointmentsCopy += ScheduleGridAppointmentsCopy;
            scheduleGrid.AppointmentsPaste += ScheduleGridAppointmentsPaste;
            scheduleGrid.GridVisualStyles = GridVisualStyles.Office2007Blue;
            scheduleGrid.ThemesEnabled = true;
            return scheduleGrid;
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

        /// <summary>
        /// Called when [schedule type change].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
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
                    //SetSevenDayWeek();
                    _previousButton.Visible = true;
                    _nextButton.Visible = true;
                }
            }
			
        }
        /// <summary>
        /// Sets the seven day week.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 1/23/2009
        /// </remarks>
        internal void SetSevenDayWeek()
        {
            Appearance.WeekHeaderFormat = "MMMM dd";
            DateSelections dates = Calendar.SelectedDates;
            DateTime firstDay = dates[0];
            //if (_previousView == ScheduleViewType.CustomWeek)
            //{
            //    //_dayIndex = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetDayOfMonth(dates[0]);
            //    AgentScheduleStateHolder.Instance().CalendarDayIndex =
            //        CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetDayOfMonth(dates[0].Date);
            //}
            //if ((_previousView == ScheduleViewType.Month))
            //{
            //    firstDay = dates[AgentScheduleStateHolder.Instance().CalendarDayIndex - 1].Date;
            //}
            //else 
            //{
            //    int thisdate = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetDayOfMonth(dates[0].Date);
            //    int difference = AgentScheduleStateHolder.Instance().CalendarDayIndex - thisdate;
            //    firstDay = dates[0].Date.AddDays(difference);
            //}         
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

            //ScheduleType = ScheduleViewType.CustomWeek;
            GetScheduleHost().SwitchTo(ScheduleViewType.CustomWeek, true);
			Appearance.ScheduleAppointmentTipsEnabled = true;
            _previousButton.Visible = true;
            _nextButton.Visible = true;
        }

        //internal void 

        public CustomScheduleControl()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Handles the KeyDown event of the ScheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachinthaw
        /// Created date: 2008-11-18
        /// </remarks>
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

        /// <summary>
        /// Handles the MouseDown event of the scheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-20
        /// </remarks>
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

        /// <summary>
        /// Handles the AppointmentsPaste event of the scheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScheduleGridAppointmentCopyEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        private void ScheduleGridAppointmentsPaste(object sender, EventArgs e)
        {
            CustomScheduleGrid scheduleGrid = (CustomScheduleGrid)GetScheduleHost();
            CustomScheduleAppointmentCopyPasteEventArgs arg = new CustomScheduleAppointmentCopyPasteEventArgs();
            OnPasteScheduleAppointment(arg);

        }

        /// <summary>
        /// Handles the AppointmentsCopy event of the scheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScheduleGridAppointmentCopyEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        private void ScheduleGridAppointmentsCopy(object sender, EventArgs e)
        {
            CustomScheduleAppointmentCopyPasteEventArgs arg = new CustomScheduleAppointmentCopyPasteEventArgs();
            OnCopyScheduleAppointment(arg);
        }

        /// <summary>
        /// Handles the Click event of the _nextButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 1/29/2009
        /// </remarks>
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

        /// <summary>
        /// Handles the Click event of the _previousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 1/29/2009
        /// </remarks>
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

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-12-04
        /// </remarks>
        private void Initialize()
        {
            _previousView = ScheduleType;
            Appearance.WeekMonthNewMonth = "d"; // Must have full date for parsing later on.
            Appearance.WeekMonthItemFormat = "[starttime] - [endtime]\n[subject]";
            Appearance.AllDayItemFormat = "[subject]";
            Appearance.DayItemFormat = "[subject] [starttime] - [endtime]\n[location]";
            //Appearance.ScheduleAppointmentTipFormat = "[subject] [starttime] - [endtime]";
            Appearance.ScheduleAppointmentTipsEnabled = true;
            //Appearance.WeekMonthItemFormat
            CaptionPanel.ContextMenu = new ContextMenu(); //Block right click
            //caption buttons
            _nextButton = CaptionPanel.Controls[1];
            _previousButton = CaptionPanel.Controls[2];
            _nextButton.ContextMenu = new ContextMenu();
            _previousButton.ContextMenu = new ContextMenu();
            _previousButton.Click += PreviousButtonClick;
            _nextButton.Click += NextButtonClick;
        }

        /// <summary>
        /// Navigates the in week view.
        /// </summary>
        /// <param name="captionButton">The caption button.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 1/29/2009
        /// </remarks>
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
            if (CopyScheduleAppointment != null)
            {
                CopyScheduleAppointment.Invoke(this, arg);
            }
        }

        protected virtual void OnPasteScheduleAppointment(CustomScheduleAppointmentCopyPasteEventArgs arg)
        {
            if (PasteScheduleAppointment != null)
            {
                PasteScheduleAppointment.Invoke(this, arg);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CustomScheduleControl
            // 
            this.Name = "CustomScheduleControl";
            this.ResumeLayout(false);

        }


    }
}
