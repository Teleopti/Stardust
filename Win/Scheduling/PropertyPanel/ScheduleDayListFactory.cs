using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public interface IScheduleDayListFactory
    {
        IList<IScheduleDay> CreatScheduleDayList();
    }

    public class ScheduleDayListFactory : IScheduleDayListFactory
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;

        public ScheduleDayListFactory(ISchedulerStateHolder schedulerStateHolder)
        {
            _schedulerStateHolder = schedulerStateHolder;
        }

        public IList<IScheduleDay> CreatScheduleDayList()
        {
            var allSchedules = new List<IScheduleDay>();
            var period = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
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