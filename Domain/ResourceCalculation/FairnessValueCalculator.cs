using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class FairnessValueCalculator : IFairnessValueCalculator
    {
        public double CalculateFairnessValue(
            double shiftValue, 
            double shiftCategoryFairnessPoint, 
            double maxFairnessPoint, 
            double totalFairness, 
            IFairnessValueResult agentFairness, 
            double maxShiftValue,
			ISchedulingOptions schedulingOptions)
        {
            var positiveMaxFairnessPoint = maxFairnessPoint > 0 ? maxFairnessPoint : 1;
            var averageFairnessPointTotal = totalFairness;
            var averageFairnessPointAgent = agentFairness.FairnessPointsPerShift;
            
			double percentFairnessPart = (1 - schedulingOptions.Fairness.Value) * shiftValue;
            double shiftCategoryFairnessPart = shiftCategoryFairness(differenceFromAverage(shiftCategoryFairnessPoint,averageFairnessPointTotal,averageFairnessPointAgent), positiveMaxFairnessPoint);

            return percentFairnessPart +
				   schedulingOptions.Fairness.Value * Math.Abs(maxShiftValue) * shiftCategoryFairnessPart;
        }

		private double shiftCategoryFairness(double differenceFromAverage, double maxFairnessPoint)
        {
            return 1 - differenceFromAverage / (2 * maxFairnessPoint);
        }

		private double differenceFromAverage(double shiftCategoryFairnessPoint, double averageFairnessPointTotal, double averageFairnessPointAgent)
        {
            double ret;
            double differenceAgent = averageFairnessPointTotal - averageFairnessPointAgent;
            if (differenceAgent >= 0)
            {
                ret = Math.Ceiling(averageFairnessPointTotal + differenceAgent);
            }
            else
            {
                ret = Math.Floor(averageFairnessPointTotal + differenceAgent);
            }
            return Math.Abs(ret - shiftCategoryFairnessPoint);
        }

    }
}
