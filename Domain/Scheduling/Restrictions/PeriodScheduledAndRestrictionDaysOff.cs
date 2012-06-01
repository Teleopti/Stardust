using System.Linq;
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
        	return (
        	       	from d in matrix.EffectivePeriodDays
					let preferences = extractor
        	       	select isDayOff(extractor, matrix.Person, d.DaySchedulePart(), useSchedules, usePreferences, useRotations)
        	       ).Sum();
        }

		private static int isDayOff(IRestrictionExtractor extractor, IPerson person, IScheduleDay scheduleDay, bool useSchedules, bool usePreferences, bool useRotations)
        {
            var significant = scheduleDay.SignificantPart();

            if (useSchedules &&
                (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff))
                return 1;

            if (useSchedules && scheduleDay.IsScheduled())
                return 0;

			extractor.Extract(person, scheduleDay.DateOnlyAsPeriod.DateOnly);
            foreach (var restriction in extractor.PreferenceList)
            {
                if (usePreferences && restriction.DayOffTemplate != null)
                {
                    return 1;
                }

                if(usePreferences && restriction.Absence != null)
                {
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