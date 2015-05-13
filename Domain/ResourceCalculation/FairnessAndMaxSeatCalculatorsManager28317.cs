using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IFairnessAndMaxSeatCalculatorsManager
	{
		IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues,
											   double maxValue, IPerson person, DateOnly dateOnly,
											   IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods, TimeSpan averageWorkTimePerDay, ISchedulingOptions schedulingOptions);
	}


	public class FairnessAndMaxSeatCalculatorsManager28317 : IFairnessAndMaxSeatCalculatorsManager
	{
		private readonly IShiftCategoryFairnessManager _shiftCategoryFairnessManager;
		private readonly IShiftCategoryFairnessShiftValueCalculator _categoryFairnessShiftValueCalculator;
		private readonly ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;

		public FairnessAndMaxSeatCalculatorsManager28317(IShiftCategoryFairnessManager shiftCategoryFairnessManager, IShiftCategoryFairnessShiftValueCalculator categoryFairnessShiftValueCalculator, ISeatLimitationWorkShiftCalculator2 seatLimitationWorkShiftCalculator)
		{
			_shiftCategoryFairnessManager = shiftCategoryFairnessManager;
			_categoryFairnessShiftValueCalculator = categoryFairnessShiftValueCalculator;
			_seatLimitationWorkShiftCalculator = seatLimitationWorkShiftCalculator;
		}

		public IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues, double maxValue, IPerson person,
			DateOnly dateOnly, IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods, TimeSpan averageWorkTimePerDay,
			ISchedulingOptions schedulingOptions)
		{
			double highestShiftValue = double.MinValue;
            IShiftCategoryFairnessFactors shiftCategoryFairnessFactors = null;
			if (schedulingOptions.Fairness.Value > 0)
			{
				shiftCategoryFairnessFactors = _shiftCategoryFairnessManager.GetFactorsForPersonOnDate(person, dateOnly);
			}

			var foundValues = new List<IWorkShiftCalculationResultHolder>();


            foreach (WorkShiftCalculationResult thisShiftValue in allValues)
            {
                IShiftProjectionCache shiftProjection = thisShiftValue.ShiftProjection;
	            double shiftValue = thisShiftValue.Value;

	            if (shiftCategoryFairnessFactors != null)
	            {          
			            IShiftCategory shiftCategory = shiftProjection.TheMainShift.ShiftCategory;
			            double factorForShiftCategory = shiftCategoryFairnessFactors.FairnessFactor(shiftCategory);
			            var fairnessValue =
				            _categoryFairnessShiftValueCalculator.ModifiedShiftValue(thisShiftValue.Value,
				                                                                     factorForShiftCategory,
				                                                                     maxValue, schedulingOptions);
		           
		            

		            shiftValue = fairnessValue;
	            }

				if (shiftValue > highestShiftValue && schedulingOptions.UserOptionMaxSeatsFeature != MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
                {
                    var seatVal =
                        _seatLimitationWorkShiftCalculator.CalculateShiftValue(person,
                                                                               shiftProjection.MainShiftProjection,
                                                                               maxSeatSkillPeriods,
																			   schedulingOptions.UserOptionMaxSeatsFeature);
                    if (!seatVal.HasValue)
                        continue;

                    shiftValue += seatVal.Value;
                }

                if (shiftValue > highestShiftValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResult { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues = new List<IWorkShiftCalculationResultHolder> { workShiftFinderResultHolder };
                    highestShiftValue = shiftValue;
                    continue;
                }
                if (Math.Abs(shiftValue - highestShiftValue) < 0.000001)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResult { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues.Add(workShiftFinderResultHolder);
                }
            }
            return foundValues;
        }
		
	}
}