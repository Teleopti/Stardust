using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public class RecurrentMonthlyByDayMeeting : RecurrentMeetingOption, IRecurrentMonthlyByDayMeeting
    {
        private int _dayInMonth = 1;

        public override IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate)
        {
            IList<DateOnly> meetingDays = new List<DateOnly>();
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            DateOnly firstDayOfMonth = new DateOnly(DateHelper.GetFirstDateInMonth(startDate.Date, calendar));

            for (int monthCount = 0; ; monthCount += IncrementCount)
            {
                DateOnly firstDayOfCurrentMonth =
                    new DateOnly(calendar.AddMonths(firstDayOfMonth.Date,
                                                                                                    monthCount));
                if (firstDayOfCurrentMonth>endDate) break;

                int dayInCurrentMonth = _dayInMonth;
                int daysInCurrentMonth = calendar.GetDaysInMonth(calendar.GetYear(firstDayOfCurrentMonth.Date),calendar.GetMonth(firstDayOfCurrentMonth.Date));
                if (daysInCurrentMonth < dayInCurrentMonth)
                    dayInCurrentMonth = daysInCurrentMonth;

                DateOnly dayOfCurrentMonth = firstDayOfCurrentMonth.AddDays(dayInCurrentMonth - 1);
                if (dayOfCurrentMonth>=startDate && dayOfCurrentMonth<=endDate)
                    meetingDays.Add(dayOfCurrentMonth);
            }
            return meetingDays;
        }

        public virtual int DayInMonth
        {
            get { return _dayInMonth; }
            set
            {
                InParameter.ValueMustBeLargerThanZero(nameof(DayInMonth), value);
                if (value > 31) throw new ArgumentOutOfRangeException(nameof(DayInMonth), "The supplied value is bigger than number of days available in a month."); 
                _dayInMonth = value;
            }
        }
    }
}