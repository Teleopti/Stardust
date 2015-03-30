using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class DayPresenter : SchedulePresenterBase
    {
        private const int _dayHeaderIndex = 0;                              //index of day header
        private const int _timeLineHeaderIndex = 1;                         //index of timeline

        public DayPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, GridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback,
            defaultScheduleTag)
        {
        }

        public int GetNowPosition(Rectangle bounds, DateOnly localDate)
        {
            DateTimePeriod period = TimelineSpan[localDate];
            var calculator = new LengthToTimeCalculator(period, bounds.Width);
            return (int) calculator.PositionFromDateTime(Now, View.IsRightToLeft) + bounds.Left;
        }

        public DateOnly GetLocalDateFromColumn(int column)
        {
            if (column<-1) throw new ArgumentOutOfRangeException("column");
            return SelectedPeriod.DateOnlyPeriod.StartDate.AddDays(column - (int)ColumnType.StartScheduleColumns);
        }

        public int GetColumnFromLocalDate(DateOnly now)
        {
            return (int)(now.Subtract(SelectedPeriod.DateOnlyPeriod.StartDate).TotalDays) + (int)ColumnType.StartScheduleColumns;
        }

        /// <summary>
        /// Creates the day header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: claess/moved by zoet
        /// Created date: 2007-11-15
        /// </remarks>
		protected override void CreateDayHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < (int)ColumnType.StartScheduleColumns)
            {
                return;
            }

            if (e.RowIndex == _dayHeaderIndex)
            {
                // Text date header
                var localDate = GetLocalDateFromColumn(e.ColIndex);
                e.Style.Text = localDate.ToShortDateString(CultureInfo.CurrentCulture);
                e.Style.Tag = localDate;
                e.Style.CellTipText =
                    DateHelper.WeekNumber(localDate.Date, CultureInfo.CurrentCulture)
                        .ToString(CultureInfo.CurrentCulture);

                View.SetCellBackTextAndBackColor(e, localDate, false, true, null);
            } 
            else if (e.RowIndex == _timeLineHeaderIndex)
            {
                // Time line
                e.Style.Tag = GetLocalDateFromColumn(e.ColIndex);
            }
        }

        /// <summary>
        /// Collect data for merging week headers
        /// </summary>
        public override void MergeHeaders()
        {
            //we dont have a week header in this view
        }

        public void TimelineList()
        {
            TimelineSpan.Clear();
            
            Dictionary<DateTime, DateTime> start = new Dictionary<DateTime, DateTime>();
            Dictionary<DateTime, DateTime> end = new Dictionary<DateTime, DateTime>();

            DateOnly periodLocalStart = SelectedPeriod.DateOnlyPeriod.StartDate;
            DateOnly periodLocalEnd = SelectedPeriod.DateOnlyPeriod.EndDate;

            foreach (KeyValuePair<Guid, IPerson> kvp in SchedulerState.FilteredPersonDictionary)
            {
                IPerson person = kvp.Value;
                IScheduleRange totalScheduleRange = SchedulerState.Schedules[person];
                //IScheduleDay schedulePart = totalScheduleRange.ScheduledPeriod(SelectedPeriod);
                IEnumerable<IScheduleDay> scheduleDays =
                    totalScheduleRange.ScheduledDayCollection(new DateOnlyPeriod(periodLocalStart, periodLocalEnd));
                foreach (var scheduleDay in scheduleDays)
                {
	                IList<IPersonAssignment> personAssignmentCollection = new List<IPersonAssignment>();
					if(scheduleDay.PersonAssignment() != null)
						personAssignmentCollection.Add(scheduleDay.PersonAssignment());

                    foreach (IPersonAssignment ag in personAssignmentCollection)
                    {
                        //find earliest start
                        DateTime startDateTimeLocal = ag.Period.LocalStartDateTime;
                        if (start.ContainsKey(startDateTimeLocal.Date))
                        {
                            if (startDateTimeLocal <= start[startDateTimeLocal.Date])
                            {
                                start[startDateTimeLocal.Date] = startDateTimeLocal;
                            }
                        }
                        else
                        {
                            start.Add(startDateTimeLocal.Date, startDateTimeLocal);
                        }

                        //find latest end
                        if (end.ContainsKey(startDateTimeLocal.Date))
                        {
	                        DateTime endDateTimeLocal = ag.Period.LocalEndDateTime;
	                        if (endDateTimeLocal >= end[startDateTimeLocal.Date])
                            {
                                //add one extra hour to end time
                                end[startDateTimeLocal.Date] = endDateTimeLocal;
                            }
                        }
                        else
                        {
                            //add one extra hour to end time
                            end.Add(startDateTimeLocal.Date, ag.Period.LocalEndDateTime);
                        }
                    }
                }
            }

            //loop each day in our selection and add timeline for each day to dictionary
            for (var currentDate = periodLocalStart; currentDate <= periodLocalEnd; currentDate = currentDate.AddDays(1))
            {
                DateTime startDateTime;
                DateTime endDateTime;

                //if we have times from assignment
                if (start.ContainsKey(currentDate.Date))
                {
                    startDateTime = start[currentDate.Date];
                    endDateTime = end[currentDate.Date];
                }
                    //if we dont have an assignment we create a default timeline
                else
                {
                    startDateTime = currentDate.Date.AddHours(8);
                    endDateTime = currentDate.Date.AddHours(17);
                }

                startDateTime = startDateTime.AddMinutes(-startDateTime.Minute).AddHours(-1);
                endDateTime = endDateTime.AddMinutes(-endDateTime.Minute).AddHours(1);

                DateTimePeriod dp = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime,endDateTime);
                TimelineSpan.Add(currentDate, dp);
            }
        }
    }
}
