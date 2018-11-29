using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public class RecurrentWeeklyMeeting : RecurrentMeetingOption, IRecurrentWeeklyMeeting
    {
	    private ISet<DayOfWeek> _weekDays = new HashSet<DayOfWeek>();

        public override IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate)
        {
            IList<DateOnly> meetingDays = new List<DateOnly>();
            for (int weekCount = 0; ; weekCount += IncrementCount)
            {
                DateOnly weekStartDate = startDate.AddDays(weekCount * 7);
                if (weekStartDate > endDate) break;
                for (DateOnly currentDate = weekStartDate; currentDate <= weekStartDate.AddDays(6); currentDate = currentDate.AddDays(1))
                {
                    if (currentDate<=endDate && this[currentDate.DayOfWeek])
                        meetingDays.Add(currentDate);
                }
            }
            return meetingDays;
        }

        public virtual IEnumerable<DayOfWeek> WeekDays
        {
            get { return _weekDays; }
        }

        public virtual bool this[DayOfWeek dayOfWeek]
        {
            get { return _weekDays.Contains(dayOfWeek); }
            set
            {
                bool dayExists = _weekDays.Contains(dayOfWeek);
                if (!dayExists && value)
                {
                    _weekDays.Add(dayOfWeek);
                }
                if (dayExists && !value)
                {
                    _weekDays.Remove(dayOfWeek);
                }
            }
        }

        protected override void  AddExtraCloneData(IRecurrentMeetingOption retObj)
        {
 	        base.AddExtraCloneData(retObj);
            var weekly = retObj as RecurrentWeeklyMeeting;
            if (weekly != null)
            {
	            weekly._weekDays = new HashSet<DayOfWeek>();
                foreach (var dayOfWeek in WeekDays)
                {
                    weekly._weekDays.Add(dayOfWeek);
                }
            }
        }
        
    }
}