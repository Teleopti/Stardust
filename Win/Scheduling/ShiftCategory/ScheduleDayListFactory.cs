using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    public interface IScheduleDayListFactory
    {
        IList<IScheduleDay> CreatScheduleDayList(DateOnlyPeriod selectedPeriod);
    }

    public class ScheduleDayListFactory : IScheduleDayListFactory
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;

        public ScheduleDayListFactory(ISchedulerStateHolder schedulerStateHolder)
        {
            _schedulerStateHolder = schedulerStateHolder;
        }

        public IList<IScheduleDay> CreatScheduleDayList(DateOnlyPeriod selectedPeriod)
        {
            var allSchedules = new List<IScheduleDay>();
            var period = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
            period = new DateOnlyPeriod(period.StartDate.AddDays(-10), period.EndDate.AddDays(10));
            var persons = _schedulerStateHolder.FilteredPersonDictionary;

            foreach (var day in period.DayCollection())
            {
                foreach (var person in persons)
                {
                    var theDay = _schedulerStateHolder.Schedules[person.Value].ScheduledDay(day);
                    allSchedules.Add(theDay);
                }
            }

            return allSchedules;
        }
    }
}