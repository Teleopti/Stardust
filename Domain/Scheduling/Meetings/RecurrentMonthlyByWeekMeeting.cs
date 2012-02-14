using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public class RecurrentMonthlyByWeekMeeting : RecurrentMeetingOption, IRecurrentMonthlyByWeekMeeting
    {
        private WeekNumber _weekOfMonth;
        private DayOfWeek _dayOfWeek;

        public override IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            IList<DateOnly> listToReturn = new List<DateOnly>();
            if (_weekOfMonth == WeekNumber.Last)
            {
                DateTime currentDate = DateHelper.GetFirstDateInMonth(startDate, calendar);
                for (; currentDate < endDate; currentDate = calendar.AddMonths(currentDate,IncrementCount))
                {
                    DateTime lastDateInMonth = DateHelper.GetLastDateInMonth(currentDate, calendar);
                    do
                    {
                        lastDateInMonth = calendar.AddDays(lastDateInMonth, -1);
                    } while (lastDateInMonth.DayOfWeek != _dayOfWeek);
                    if (lastDateInMonth>startDate.Date && lastDateInMonth<endDate.Date)
                        listToReturn.Add(new DateOnly(lastDateInMonth));
                }
            }
            else
            {
                int addDaysForWeekNumber = GetDaysToAddForWeekNumber(_weekOfMonth);
                DateTime currentDate = DateHelper.GetFirstDateInMonth(startDate, calendar);
                for (; currentDate < endDate; currentDate = calendar.AddMonths(currentDate, IncrementCount))
                {
                    int dayCount = (_weekOfMonth==WeekNumber.First) ? 0 : 1;
                    DateTime startingDay = calendar.AddDays(currentDate, addDaysForWeekNumber);
                    do
                    {
                        startingDay = calendar.AddDays(startingDay, dayCount);
                        dayCount = 1;
                    } while (startingDay.DayOfWeek != _dayOfWeek);
                    if (startingDay >= startDate.Date && startingDay <= endDate.Date)
                        listToReturn.Add(new DateOnly(startingDay));
                }
            }
            return listToReturn;
        }

        private static int GetDaysToAddForWeekNumber(WeekNumber weekNumber)
        {
            return Math.Max((int)weekNumber * 7, 1) -1;
        }

        public virtual WeekNumber WeekOfMonth
        {
            get { return _weekOfMonth; }
            set { _weekOfMonth = value; }
        }

        public virtual DayOfWeek DayOfWeek
        {
            get { return _dayOfWeek; }
            set { _dayOfWeek = value; }
        }
    }
}