using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToSelectedSchedules : ISetupDateTimePeriod
    {
        private readonly DateTimePeriod _period;

        public SetupDateTimePeriodToSelectedSchedules(IList<IScheduleDay> scheduleDays)
        {
            _period = GetPeriodFromScheduleDays(scheduleDays);
        }

        private static DateTimePeriod GetPeriodFromScheduleDays(IList<IScheduleDay> scheduleDays)
        {
            if (IsOneScheduleDayWithMainShiftProjection(scheduleDays))
            {
                ISetupDateTimePeriod setupDateTimePeriodToDefaultLocalHours =
                    new SetupDateTimePeriodDefaultLocalHoursForActivities(scheduleDays[0]);

                SetupDateTimePeriodToSelectedSchedule setupDateTimePeriodToSelectedSchedule =
                    new SetupDateTimePeriodToSelectedSchedule(scheduleDays[0], setupDateTimePeriodToDefaultLocalHours);

                return setupDateTimePeriodToSelectedSchedule.Period;
            }

            IList<DateTimePeriod> periods = new List<DateTimePeriod>();
            foreach (IScheduleDay scheduleDay in scheduleDays)
            {
                periods.Add(scheduleDay.Period);
            }

            CheckForEmptyPeriodList(periods);
            return new DateTimePeriod(periods.Min(p => p.StartDateTime), periods.Max(p => p.EndDateTime.Subtract(TimeSpan.FromMinutes(1))));
        }

        private static void CheckForEmptyPeriodList(IList<DateTimePeriod> periods)
        {
            if (periods.Count==0)
            {
                throw new InvalidOperationException("No periods was found using this setup period type.");
            }
        }

        private static bool IsOneScheduleDayWithMainShiftProjection(IEnumerable<IScheduleDay> scheduleDays)
        {
            if (scheduleDays.Count() > 1) return false;
            var scheduleDay = scheduleDays.SingleOrDefault();
            if (scheduleDay == null) return false;

            var assignment = scheduleDay.AssignmentHighZOrder();
            return assignment!=null && assignment.ToMainShift()!=null && assignment.ToMainShift().HasProjection;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }
}
