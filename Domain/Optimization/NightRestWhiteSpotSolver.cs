using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface INightRestWhiteSpotSolver
    {
        NightRestWhiteSpotSolverResult Resolve(IScheduleMatrixPro matrix);
    }

    public class NightRestWhiteSpotSolver : INightRestWhiteSpotSolver
    {
        public NightRestWhiteSpotSolverResult Resolve(IScheduleMatrixPro matrix)
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
                if (matrix.GetScheduleDayByKey(matrixEffectivePeriodDay.Day.AddDays(-1)).DaySchedulePart().IsScheduled())
                    result.AddDayToDelete(matrixEffectivePeriodDay.Day.AddDays(-1));
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