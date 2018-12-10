using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class NightRestWhiteSpotSolver
    {
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public NightRestWhiteSpotSolver(IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}
        public NightRestWhiteSpotSolverResult Resolve(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
            NightRestWhiteSpotSolverResult result = new NightRestWhiteSpotSolverResult();

            for (int i = 1; i < matrix.EffectivePeriodDays.Length; i++)
            {
	            var matrixEffectivePeriodDay = matrix.EffectivePeriodDays[i];
	            if (matrixEffectivePeriodDay.DaySchedulePart().IsScheduled())
                    continue;

                bool multipleFound = false;
                while (i < matrix.EffectivePeriodDays.Length && !matrix.GetScheduleDayByKey(matrix.EffectivePeriodDays[i].Day.AddDays(1)).DaySchedulePart().IsScheduled())
                {
                    i++;
                    multipleFound = true;
                }

                if (multipleFound)
                    continue;

	            var previousMatrixEffectivePeriodDay = matrix.EffectivePeriodDays[i - 1];
	            if (!matrix.UnlockedDays.Contains(previousMatrixEffectivePeriodDay))
                    continue;

                if (!matrix.UnlockedDays.Contains(matrixEffectivePeriodDay))
                    continue;

                if (previousMatrixEffectivePeriodDay.DaySchedulePart().SignificantPart() == SchedulePartView.DayOff)
                    continue;

				var previousScheduleDay = matrix.GetScheduleDayByKey(matrixEffectivePeriodDay.Day.AddDays(-1)).DaySchedulePart();
				var effectiveRestrictions =_effectiveRestrictionCreator.GetEffectiveRestriction(previousScheduleDay, schedulingOptions);
				var removePreviousDay = previousScheduleDay.IsScheduled();

				if (effectiveRestrictions != null)
				{
					if (schedulingOptions.UsePreferences && effectiveRestrictions.Absence != null &&
						effectiveRestrictions.IsPreferenceDay)
						removePreviousDay = false;

					if ((schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly) &&
						schedulingOptions.UsePreferences && !effectiveRestrictions.IsPreferenceDay)
						removePreviousDay = false;

					if (schedulingOptions.AvailabilityDaysOnly && schedulingOptions.UseAvailability &&
						!effectiveRestrictions.IsAvailabilityDay)
						removePreviousDay = false;

					if (schedulingOptions.RotationDaysOnly && schedulingOptions.UseRotations && !effectiveRestrictions.IsRotationDay)
						removePreviousDay = false;
				}

				if (removePreviousDay) result.AddDayToDelete(matrixEffectivePeriodDay.Day.AddDays(-1));
                result.AddDayToReschedule(matrixEffectivePeriodDay.Day.AddDays(-1));
				result.AddDayToReschedule(matrixEffectivePeriodDay.Day);
			}

            return result;
        }
    }

    public class NightRestWhiteSpotSolverResult
    {
        private readonly IList<DateOnly> _daysToDelete;
        private readonly IList<DateOnly> _daysToReschedule;

        public NightRestWhiteSpotSolverResult()
        {
            _daysToDelete = new List<DateOnly>();
            _daysToReschedule = new List<DateOnly>();
        }

        public IEnumerable<DateOnly> DaysToDelete => _daysToDelete;

	    public IEnumerable<DateOnly> DaysToReschedule()
	    {
		    return _daysToReschedule.OrderByDescending(d => d);
	    }

        public void AddDayToReschedule(DateOnly dateOnly)
        {
            _daysToReschedule.Add(dateOnly);
        }

	    public void AddDayToDelete(DateOnly dateOnly)
	    {
		    _daysToDelete.Add(dateOnly);
	    }
    }
}