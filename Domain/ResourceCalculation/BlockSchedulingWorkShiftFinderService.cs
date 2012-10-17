using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IBlockSchedulingWorkShiftFinderService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ShiftProjectionShiftValue BestShiftValue(DateOnly dateOnly, IList<IShiftProjectionCache> filteredListOfShifts, IDictionary<IActivity, IDictionary<DateTime,
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

        

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "8"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public ShiftProjectionShiftValue BestShiftValue(DateOnly dateOnly, IList<IShiftProjectionCache> filteredListOfShifts, IDictionary<IActivity,
            IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, IFairnessValueResult totalFairness,IFairnessValueResult agentFairness,
			int maxFairnessOnShiftCategories, TimeSpan averageWorkTime, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons, ISchedulingOptions schedulingOptions)
        {
            IList<ShiftProjectionShiftValue> allValues = new List<ShiftProjectionShiftValue>(filteredListOfShifts.Count);
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
		    ShiftProjectionShiftValue finalShiftProjectionShift = new ShiftProjectionShiftValue();
            foreach (IShiftProjectionCache shiftProjection in filteredListOfShifts)
            {
                double thisValue = _calculator.CalculateShiftValue(shiftProjection.MainShiftProjection, dataHolders,lengthFactor, useMinimumPersons, useMaximumPersons);
                var shiftValue = new ShiftProjectionShiftValue { ShiftProjection = shiftProjection, Value = thisValue };
                allValues.Add(shiftValue);
                if (thisValue > maxValue)
                    maxValue = thisValue;
                if (thisValue < minValue)
                    minValue = thisValue;
            }
                                                                
            double highestShiftValue = double.MinValue;
            IShiftProjectionCache foundShift = null;

            foreach (ShiftProjectionShiftValue thisShiftValue in allValues)
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
                    foundShift = shiftProjection;
                    highestShiftValue = shiftValue;
                }

            }
           
		    if (highestShiftValue == double.MinValue)
                return new ShiftProjectionShiftValue {  Value = double.MinValue };
            if (foundShift == null)
                return new ShiftProjectionShiftValue {  Value =  double.MinValue};
            if (schedulingOptions != null && schedulingOptions.UseCommonActivity)
            {
                finalShiftProjectionShift.ActivityPeriod = new List<DateTimePeriod>();
                foreach (var proj in foundShift.TheMainShift.ProjectionService().CreateProjection())
                {
                    if (proj.Payload.Id == schedulingOptions.CommonActivity.Id)
                    {
                        finalShiftProjectionShift.ActivityPeriod.Add(proj.Period);
                    }

                }
            }
            finalShiftProjectionShift.Value = highestShiftValue;
            return  finalShiftProjectionShift; 
        }
        
        
    }

    public class ShiftProjectionShiftValue
    {
        public double Value { get; set; }
        public IShiftProjectionCache ShiftProjection { get; set; }
        public IList<DateTimePeriod> ActivityPeriod { get; set; }
    }
}
