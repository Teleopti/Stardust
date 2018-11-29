using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
                var currentDate = new DateOnly(DateHelper.GetFirstDateInMonth(startDate.Date, calendar));
                for (; currentDate < endDate; currentDate = new DateOnly(calendar.AddMonths(currentDate.Date,IncrementCount)))
                {
                    DateTime lastDateInMonth = DateHelper.GetLastDateInMonth(currentDate.Date, calendar);
					while (lastDateInMonth.DayOfWeek != _dayOfWeek)
					{
                        lastDateInMonth = calendar.AddDays(lastDateInMonth, -1);
                    }
                    if (lastDateInMonth>=startDate.Date && lastDateInMonth<=endDate.Date)
                        listToReturn.Add(new DateOnly(lastDateInMonth));
                }
            }
            else
            {
                int addDaysForWeekNumber = GetDaysToAddForWeekNumber(_weekOfMonth);
                var currentDate = new DateOnly(DateHelper.GetFirstDateInMonth(startDate.Date, calendar));
                for (; currentDate < endDate; currentDate = new DateOnly(calendar.AddMonths(currentDate.Date, IncrementCount)))
                {
                    int dayCount = (_weekOfMonth==WeekNumber.First) ? 0 : 1;
                    DateTime startingDay = calendar.AddDays(currentDate.Date, addDaysForWeekNumber);
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