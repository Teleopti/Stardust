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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public NightRestWhiteSpotSolverResult Resolve(IScheduleMatrixPro matrix)
        {
            NightRestWhiteSpotSolverResult result = new NightRestWhiteSpotSolverResult();

            for (int i = 1; i < matrix.EffectivePeriodDays.Count; i++)
            {

                if (matrix.EffectivePeriodDays[i].DaySchedulePart().IsScheduled())
                    continue;

                bool multipleFound = false;
                while (i < matrix.EffectivePeriodDays.Count && !matrix.GetScheduleDayByKey(matrix.EffectivePeriodDays[i].Day.AddDays(1)).DaySchedulePart().IsScheduled())
                {
                    i++;
                    multipleFound = true;
                }

                if (multipleFound)
                    continue;

                if (!matrix.UnlockedDays.Contains(matrix.EffectivePeriodDays[i - 1]))
                    continue;

                if (!matrix.UnlockedDays.Contains(matrix.EffectivePeriodDays[i]))
                    continue;

                if (matrix.EffectivePeriodDays[i - 1].DaySchedulePart().SignificantPart() == SchedulePartView.DayOff)
                    continue;

                result.DaysToDelete.Add(matrix.EffectivePeriodDays[i].Day.AddDays(-1));
                result.AddDayToReschedule(matrix.EffectivePeriodDays[i].Day.AddDays(-1));
                result.AddDayToReschedule(matrix.EffectivePeriodDays[i].Day);
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

        public IList<DateOnly> DaysToDelete
        {
            get { return _daysToDelete; }
        }

        public IList<DateOnly> DaysToReschedule()
        {
            return _daysToReschedule.OrderByDescending(d => d).ToList(); 
        }

        public void AddDayToReschedule(DateOnly dateOnly)
        {
            _daysToReschedule.Add(dateOnly);
        }

        
    }
}