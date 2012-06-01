using System.Collections.Generic;
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
        	var extractOperation = new RestrictionExtractOperation();
        	var scheduleDayWithRestrictions = from d in matrix.EffectivePeriodDays
        	                                  let scheduleDay = d.DaySchedulePart()
        	                                  let restrictions = scheduleDay.RestrictionCollection()
        	                                  let rotationRestrictions = extractOperation.GetRotationRestrictions(restrictions)
        	                                  let preferenceRestrictions = extractOperation.GetPreferenceRestrictions(restrictions)
        	                                  select new
        	                                         	{
        	                                         		scheduleDay,
        	                                         		rotationRestrictions,
        	                                         		preferenceRestrictions
        	                                         	};
        	return (
        	       	from d in scheduleDayWithRestrictions
        	       	select isDayOff(matrix.Person, d.scheduleDay, d.preferenceRestrictions, d.rotationRestrictions, useSchedules, usePreferences, useRotations)
        	       )
        		.Sum();
        }

		private static int isDayOff(IPerson person, IScheduleDay scheduleDay, IEnumerable<IPreferenceRestriction> preferenceRestrictions, IEnumerable<IRotationRestriction> rotationRestrictions,  bool useSchedules, bool usePreferences, bool useRotations)
        {
            var significant = scheduleDay.SignificantPart();

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

                    if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate))
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