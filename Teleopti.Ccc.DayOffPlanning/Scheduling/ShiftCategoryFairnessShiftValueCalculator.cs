using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{

    public class ShiftCategoryFairnessShiftValueCalculator : IShiftCategoryFairnessShiftValueCalculator
    {
        private readonly ISchedulingOptions _options;
        
        public ShiftCategoryFairnessShiftValueCalculator(ISchedulingOptions options)
        {
            _options = options;
       }

        private Percent PercentFairness
        {
            get { return _options.Fairness; }
        }

        public double ModifiedShiftValue(double shiftValue, double factorForShiftEvaluation, double maxShiftValue)
        {
            return BusinessPercent() * shiftValue +
                   PercentFairness.Value * factorForShiftEvaluation * Math.Abs(maxShiftValue);
        }

        private double BusinessPercent()
        {
            return 1 - PercentFairness.Value;
        }
    }
}