using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IPersonShiftCategoryPointCalculator
    {
        double CalculateShiftCategorySeniorityValue(IScheduleRange scheduleRange, DateOnlyPeriod visiblePeriod, IList<IShiftCategory> shiftCategories);
    }

    public class PersonShiftCategoryPointCalculator : IPersonShiftCategoryPointCalculator
    {
        private readonly IShiftCategoryPoints _shiftCategoryPoints; 

        public PersonShiftCategoryPointCalculator(IShiftCategoryPoints shiftCategoryPoints)
        {
            _shiftCategoryPoints = shiftCategoryPoints;
        }

        public double CalculateShiftCategorySeniorityValue(IScheduleRange scheduleRange, DateOnlyPeriod visiblePeriod, IList<IShiftCategory> shiftCategories)
        {
            var shiftCategoryPoints = _shiftCategoryPoints.ExtractShiftCategoryPoints(shiftCategories);
            double totalPoints = 0;
            foreach (var dateOnly in visiblePeriod.DayCollection())
            {
                var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
                if (scheduleDay.PersonAssignment() == null) continue;
                var shiftCatergoryOnScheduleDay = scheduleDay.PersonAssignment().ShiftCategory;
				int shiftCategoryPoint;
				if (shiftCatergoryOnScheduleDay != null && shiftCategoryPoints.TryGetValue(shiftCatergoryOnScheduleDay, out shiftCategoryPoint))
                {
                    totalPoints += shiftCategoryPoint;
                }
            }

            return totalPoints;
        }
    }
}
