using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{

    public class ShiftCategoryFairnessShiftValueCalculator : IShiftCategoryFairnessShiftValueCalculator
    {
        
        private static Percent percentFairness(ISchedulingOptions schedulingOptions)
        {
        	return schedulingOptions.Fairness;
        }

        public double ModifiedShiftValue(double shiftValue, double factorForShiftEvaluation, double maxShiftValue, ISchedulingOptions schedulingOptions)
        {
			return businessPercent(schedulingOptions) * shiftValue +
				   percentFairness(schedulingOptions).Value * factorForShiftEvaluation * Math.Abs(maxShiftValue);
        }

		private static double businessPercent(ISchedulingOptions schedulingOptions)
        {
			return 1 - percentFairness(schedulingOptions).Value;
        }
    }
}