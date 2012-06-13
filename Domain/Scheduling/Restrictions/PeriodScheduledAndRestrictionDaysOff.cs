using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class PeriodScheduledAndRestrictionDaysOff : IPeriodScheduledAndRestrictionDaysOff
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public int CalculatedDaysOff(IRestrictionExtractor extractor, IScheduleMatrixPro matrix, bool useSchedules, bool usePreferences, bool useRotations)
        {
            int result = 0;
            foreach (IScheduleDayPro day in matrix.EffectivePeriodDays)
            {
                result += isDayOff(extractor, matrix, day, useSchedules, usePreferences, useRotations);
            }

            return result;
        }

        private static int isDayOff(IRestrictionExtractor extractor, IScheduleMatrixPro matrix, IScheduleDayPro day, bool useSchedules, bool usePreferences, bool useRotations)
        {
            IScheduleDay scheduleDay = day.DaySchedulePart();
            SchedulePartView significant = scheduleDay.SignificantPart();

            if (useSchedules &&
                (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff))
                return 1;

            if (useSchedules && scheduleDay.IsScheduled())
                return 0;

            extractor.Extract(matrix.Person, day.Day);
            foreach (IPreferenceRestriction restriction in extractor.PreferenceList)
            {
                if (usePreferences && restriction.DayOffTemplate != null)
                {
                    return 1;
                }

                if(usePreferences && restriction.Absence != null)
                {
                    var person = scheduleDay.Person;
                    var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
                    var personPeriod = person.Period(scheduleDate);
                	var periodStart = person.SchedulePeriodStartDate(scheduleDate);
					if (!periodStart.HasValue)
						return 0;

                    if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(periodStart.Value, scheduleDate))
                        return 1;
                }
            }

            foreach (IRotationRestriction restriction in extractor.RotationList)
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