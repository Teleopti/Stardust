using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Interfaces.Domain;

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
                if (shiftCatergoryOnScheduleDay != null && shiftCategoryPoints.ContainsKey(shiftCatergoryOnScheduleDay))
                {
                    var shiftCategoryPoint = shiftCategoryPoints[shiftCatergoryOnScheduleDay];
                    totalPoints += shiftCategoryPoint;

                }
            }

            return totalPoints;
        }
    }
}
