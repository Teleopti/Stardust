using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class FairnessValueCalculator : IFairnessValueCalculator
    {
        private readonly ISchedulingOptions _options;
        private double _maxFairnessPoint;
        private double _averageFairnessPointTotal;
        private double _averageFairnessPointAgent;
        private double _maxShiftValue;

        public FairnessValueCalculator(ISchedulingOptions options)
        {
            _options = options;
        }

        private Percent PercentFairness
        {
            get { return _options.Fairness; }
        }
        public double CalculateFairnessValue(
            double shiftValue, 
            double shiftCategoryFairnessPoint, 
            double maxFairnessPoint, 
            double totalFairness, 
            IFairnessValueResult agentFairness, 
            double maxShiftValue)
        {
            _maxFairnessPoint = maxFairnessPoint > 0 ? maxFairnessPoint : 1;
            _averageFairnessPointTotal = totalFairness;
            _averageFairnessPointAgent = agentFairness.FairnessPointsPerShift;
            _maxShiftValue = maxShiftValue;
            return this.totalFairness(shiftValue, shiftCategoryFairnessPoint);
        }

        private double totalFairness(double shiftValue, double shiftCategoryFairnessPoint)
        {
            double percentFairnessPart = (1 - PercentFairness.Value) * shiftValue;
            double shiftCategoryFairnessPart = shiftCategoryFairness(shiftCategoryFairnessPoint);

            return percentFairnessPart +
                   PercentFairness.Value * Math.Abs(_maxShiftValue) * shiftCategoryFairnessPart;
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
