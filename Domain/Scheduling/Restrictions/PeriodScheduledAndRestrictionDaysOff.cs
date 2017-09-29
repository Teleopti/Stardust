using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class PeriodScheduledAndRestrictionDaysOff : IPeriodScheduledAndRestrictionDaysOff
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public int CalculatedDaysOff(IScheduleMatrixPro matrix, bool useSchedules, bool usePreferences, bool useRotations)
		{
			var scheduleDays = from d in matrix.EffectivePeriodDays select d.DaySchedulePart();
			return CalculatedDaysOff(scheduleDays, useSchedules, usePreferences, useRotations);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public int CalculatedDaysOff(IEnumerable<IScheduleDay> scheduleDays, bool useSchedules, bool usePreferences, bool useRotations)
        {
        	return (
					from d in scheduleDays
        	       	select isDayOff(d, useSchedules, usePreferences, useRotations)
        	       )
        		.Sum();
        }

		private static int isDayOff(IScheduleDay scheduleDay, bool useSchedules, bool usePreferences, bool useRotations)
        {
        	var extractOperation = new RestrictionRetrievalOperation();
            var significant = scheduleDay.SignificantPart();
			var person = scheduleDay.Person;
			var restrictions = scheduleDay.RestrictionCollection();
			var preferenceRestrictions = extractOperation.GetPreferenceRestrictions(restrictions);
			var rotationRestrictions = extractOperation.GetRotationRestrictions(restrictions);

            if (useSchedules &&
                (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff))
                return 1;

            if (useSchedules && scheduleDay.IsScheduled())
                return 0;

			foreach (var restriction in preferenceRestrictions)
            {
                if (usePreferences && restriction.DayOffTemplate != null)
                {
                    return 1;
                }

                if(usePreferences && restriction.Absence != null)
                {
                    var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
                    var personPeriod = person.Period(scheduleDate);
                	var periodStart = person.SchedulePeriodStartDate(scheduleDate);
					if (!periodStart.HasValue)
						return 0;

                    if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(periodStart.Value, scheduleDate,person.FirstDayOfWeek))
                        return 1;
                }
            }

            foreach (var restriction in rotationRestrictions)
            {
                if (restriction.DayOffTemplate != null && useRotations)
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}