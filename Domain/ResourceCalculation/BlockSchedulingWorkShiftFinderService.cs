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
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons, ISchedulingOptions schedulingOptions);
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

        private class shiftValue
        {
            public double Value { get; set; }
            public IShiftProjectionCache ShiftProjection { get; set; }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "8"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public double BestShiftValue(DateOnly dateOnly, IList<IShiftProjectionCache> filteredListOfShifts, IDictionary<IActivity,
            IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, IFairnessValueResult totalFairness,IFairnessValueResult agentFairness,
			int maxFairnessOnShiftCategories, TimeSpan averageWorkTime, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons, ISchedulingOptions schedulingOptions)
        {
            IList<shiftValue> allValues = new List<shiftValue>(filteredListOfShifts.Count);
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            foreach (IShiftProjectionCache shiftProjection in filteredListOfShifts)
            {
                double thisValue = _calculator.CalculateShiftValue(shiftProjection.MainShiftProjection, dataHolders,lengthFactor, useMinimumPersons, useMaximumPersons);
                var shiftValue = new shiftValue { ShiftProjection = shiftProjection, Value = thisValue };
                allValues.Add(shiftValue);
                if (thisValue > maxValue)
                    maxValue = thisValue;
                if (thisValue < minValue)
                    minValue = thisValue;
            }
                                                                
            double highestShiftValue = double.MinValue;
            IList<IShiftProjectionCache> foundShifts = new List<IShiftProjectionCache>();

            foreach (shiftValue thisShiftValue in allValues)
            {
                IShiftProjectionCache shiftProjection = thisShiftValue.ShiftProjection;
            	double shiftValue;
				if (useShiftCategoryFairness)
				{
					IShiftCategory shiftCategory = shiftProjection.TheMainShift.ShiftCategory;
					double factorForShiftCategory = shiftCategoryFairnessFactors.FairnessFactor(shiftCategory);
					shiftValue = _shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(thisShiftValue.Value, factorForShiftCategory, maxValue, schedulingOptions);
				}
				else
				{
					int dayOfWeekJusticeValue =
					shiftProjection.TheMainShift.ShiftCategory.DayOfWeekJusticeValues[shiftProjection.DayOfWeek];

					shiftValue = _fairnessValueCalculator.CalculateFairnessValue(
                        thisShiftValue.Value, dayOfWeekJusticeValue, maxFairnessOnShiftCategories,
                        totalFairness.FairnessPoints, agentFairness, maxValue, schedulingOptions);
				}
                
                if (shiftValue > highestShiftValue)
                {
                    foundShifts = new List<IShiftProjectionCache> { shiftProjection };
                    highestShiftValue = shiftValue;
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
