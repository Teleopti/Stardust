using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class FairnessValueCalculator : IFairnessValueCalculator
    {
        private double _maxFairnessPoint;
        private double _averageFairnessPointTotal;
        private double _averageFairnessPointAgent;
        private double _maxShiftValue;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public double CalculateFairnessValue(
            double shiftValue, 
            double shiftCategoryFairnessPoint, 
            double maxFairnessPoint, 
            double totalFairness, 
            IFairnessValueResult agentFairness, 
            double maxShiftValue,
			ISchedulingOptions schedulingOptions)
        {
            _maxFairnessPoint = maxFairnessPoint > 0 ? maxFairnessPoint : 1;
            _averageFairnessPointTotal = totalFairness;
            _averageFairnessPointAgent = agentFairness.FairnessPointsPerShift;
            _maxShiftValue = maxShiftValue;
            return this.totalFairness(shiftValue, shiftCategoryFairnessPoint, schedulingOptions);
        }

        private double totalFairness(double shiftValue, double shiftCategoryFairnessPoint, ISchedulingOptions schedulingOptions)
        {
			double percentFairnessPart = (1 - schedulingOptions.Fairness.Value) * shiftValue;
            double shiftCategoryFairnessPart = shiftCategoryFairness(shiftCategoryFairnessPoint);

            return percentFairnessPart +
				   schedulingOptions.Fairness.Value * Math.Abs(_maxShiftValue) * shiftCategoryFairnessPart;
        }

        private double shiftCategoryFairness(double shiftCategoryFairnessPoint)
        {
            return 1 - differenceFromAverage(shiftCategoryFairnessPoint) / (2 * _maxFairnessPoint);
        }

        private double differenceFromAverage(double shiftCategoryFairnessPoint)
        {
            double ret;
            double differenceAgent = _averageFairnessPointTotal - _averageFairnessPointAgent;
            if (differenceAgent >= 0)
            {
                ret = Math.Ceiling(_averageFairnessPointTotal + differenceAgent);
            }
            else
            {
                ret = Math.Floor(_averageFairnessPointTotal + differenceAgent);
            }
            return Math.Abs(ret - shiftCategoryFairnessPoint);
        }

    }
}
