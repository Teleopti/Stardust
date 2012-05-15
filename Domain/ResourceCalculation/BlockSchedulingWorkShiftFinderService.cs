using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IBlockSchedulingWorkShiftFinderService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        double BestShiftValue(DateOnly dateOnly, IList<IShiftProjectionCache> filteredListOfShifts, IDictionary<IActivity, IDictionary<DateTime,
            ISkillStaffPeriodDataHolder>> dataHolders, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness,
			int maxFairnessOnShiftCategories, TimeSpan averageWorkTime, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons);
    }

    public class BlockSchedulingWorkShiftFinderService : IBlockSchedulingWorkShiftFinderService
    {
        private readonly IWorkShiftCalculator _calculator;
        private readonly IFairnessValueCalculator _fairnessValueCalculator;
    	private readonly IShiftCategoryFairnessShiftValueCalculator _shiftCategoryFairnessShiftValueCalculator;

    	public BlockSchedulingWorkShiftFinderService(
            IWorkShiftCalculator calculator, 
            IFairnessValueCalculator fairnessValueCalculator, 
			IShiftCategoryFairnessShiftValueCalculator shiftCategoryFairnessShiftValueCalculator)
        {
            _calculator = calculator;
            _fairnessValueCalculator = fairnessValueCalculator;
        	_shiftCategoryFairnessShiftValueCalculator = shiftCategoryFairnessShiftValueCalculator;
        }

        private class ShiftValue
        {
            public double Value { get; set; }
            public IShiftProjectionCache ShiftProjection { get; set; }
        }

        public double BestShiftValue(DateOnly dateOnly, IList<IShiftProjectionCache> filteredListOfShifts, IDictionary<IActivity,
            IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, IFairnessValueResult totalFairness,IFairnessValueResult agentFairness,
			int maxFairnessOnShiftCategories, TimeSpan averageWorkTime, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
        {
            IList<ShiftValue> allValues = new List<ShiftValue>(filteredListOfShifts.Count);
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            foreach (IShiftProjectionCache shiftProjection in filteredListOfShifts)
            {
                double thisValue = _calculator.CalculateShiftValue(shiftProjection.MainShiftProjection, dataHolders,lengthFactor, useMinimumPersons, useMaximumPersons);
                ShiftValue shiftValue = new ShiftValue { ShiftProjection = shiftProjection, Value = thisValue };
                allValues.Add(shiftValue);
                if (thisValue > maxValue)
                    maxValue = thisValue;
                if (thisValue < minValue)
                    minValue = thisValue;
            }
                                                                
            double highestShiftValue = double.MinValue;
            IList<IShiftProjectionCache> foundShifts = new List<IShiftProjectionCache>();

            foreach (ShiftValue thisShiftValue in allValues)
            {
                IShiftProjectionCache shiftProjection = thisShiftValue.ShiftProjection;
            	double shiftValue;
				if (useShiftCategoryFairness)
				{
					IShiftCategory shiftCategory = shiftProjection.TheMainShift.ShiftCategory;
					double factorForShiftCategory = shiftCategoryFairnessFactors.FairnessFactor(shiftCategory);
					shiftValue = _shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(thisShiftValue.Value, factorForShiftCategory, maxValue);
				}
				else
				{
					int dayOfWeekJusticeValue =
					shiftProjection.TheMainShift.ShiftCategory.DayOfWeekJusticeValues[shiftProjection.DayOfWeek];

					shiftValue = _fairnessValueCalculator.CalculateFairnessValue(
                        thisShiftValue.Value, dayOfWeekJusticeValue, maxFairnessOnShiftCategories,
                        totalFairness.FairnessPoints, agentFairness, maxValue);
				}
                
                if (shiftValue > highestShiftValue)
                {
                    foundShifts = new List<IShiftProjectionCache> { shiftProjection };
                    highestShiftValue = shiftValue;
                    continue;
                }

            }


            if (highestShiftValue == double.MinValue)
                return double.MinValue;

            if (foundShifts.Count == 0)
                return double.MinValue;

            return highestShiftValue;
        }

    }
}
