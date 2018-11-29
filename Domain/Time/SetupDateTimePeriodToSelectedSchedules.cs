using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToSelectedSchedules : ISetupDateTimePeriod
    {
        private readonly DateTimePeriod _period;

        public SetupDateTimePeriodToSelectedSchedules(IList<IScheduleDay> scheduleDays)
        {
            _period = getPeriodFromScheduleDays(scheduleDays);
        }

        private static DateTimePeriod getPeriodFromScheduleDays(IList<IScheduleDay> scheduleDays)
        {
            if (isOneScheduleDayWithMainShiftProjection(scheduleDays))
			{
				ISetupDateTimePeriod setupDateTimePeriodToDefaultLocalHours =
					new SetupDateTimePeriodDefaultLocalHoursForActivities(scheduleDays[0], UserTimeZone.Make());

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

        private static bool isOneScheduleDayWithMainShiftProjection(IEnumerable<IScheduleDay> scheduleDays)
        {
            if (scheduleDays.Count() > 1) return false;
            var scheduleDay = scheduleDays.SingleOrDefault();

			var shift = scheduleDay?.GetEditorShift();
			return shift != null;
        }

        public DateTimePeriod Period => _period;
	}
}
