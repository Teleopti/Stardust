using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{

	public interface ISchedulePeriodPossibleResultDayOffCalculator
	{
		int PossibleResultDayOff();
	}

	public class SchedulePeriodPossibleResultDayOffCalculator : ISchedulePeriodPossibleResultDayOffCalculator
	{
		public int PossibleResultDayOff() { return 0; }
	}

	public class PeriodScheduledAndRestrictionDaysOff : IPeriodScheduledAndRestrictionDaysOff
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public int CalculatedDaysOff(IRestrictionExtractor extractor, IScheduleMatrixPro matrix, bool useSchedules, bool usePreferences, bool useRotations)
        {
            int result = 0;
            foreach (IScheduleDayPro day in matrix.EffectivePeriodDays)
            {
                result += isDayOff(extractor, matrix.Person, day.DaySchedulePart(), day.Day, useSchedules, usePreferences, useRotations);
            }

            return result;
        }

		private static int isDayOff(IRestrictionExtractor extractor, IPerson person, IScheduleDay scheduleDay, DateOnly date, bool useSchedules, bool usePreferences, bool useRotations)
        {
            var significant = scheduleDay.SignificantPart();

            if (useSchedules &&
                (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff))
                return 1;

            if (useSchedules && scheduleDay.IsScheduled())
                return 0;

			extractor.Extract(person, date);
            foreach (var restriction in extractor.PreferenceList)
            {
                if (usePreferences && restriction.DayOffTemplate != null)
                {
                    return 1;
                }

                if(usePreferences && restriction.Absence != null)
                {
                	var person2 = scheduleDay.Person;
                    var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
                    var personPeriod = person.Period(scheduleDate);

                    if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate))
                        return 1;
                }
            }

            foreach (var restriction in extractor.RotationList)
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