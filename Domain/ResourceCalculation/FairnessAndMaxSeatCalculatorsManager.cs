using System;
using System.Collections.Generic;
using System.Linq;
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

    public class FairnessAndMaxSeatCalculatorsManager : IFairnessAndMaxSeatCalculatorsManager
    {
        private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
        private readonly IShiftCategoryFairnessManager _shiftCategoryFairnessManager;
        private readonly IShiftCategoryFairnessShiftValueCalculator _categoryFairnessShiftValueCalculator;
        private readonly IFairnessValueCalculator _fairnessValueCalculator;
        private readonly ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;

        public FairnessAndMaxSeatCalculatorsManager(Func<ISchedulingResultStateHolder> resultStateHolder,
                                    IShiftCategoryFairnessManager shiftCategoryFairnessManager,
                                    IShiftCategoryFairnessShiftValueCalculator categoryFairnessShiftValueCalculator,
                                    IFairnessValueCalculator fairnessValueCalculator,
                                    ISeatLimitationWorkShiftCalculator2 seatLimitationWorkShiftCalculator)
        {
            _resultStateHolder = resultStateHolder;
            _shiftCategoryFairnessManager = shiftCategoryFairnessManager;
            _categoryFairnessShiftValueCalculator = categoryFairnessShiftValueCalculator;
            _fairnessValueCalculator = fairnessValueCalculator;
            _seatLimitationWorkShiftCalculator = seatLimitationWorkShiftCalculator;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "7"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues,
                                                    double maxValue, IPerson person, DateOnly dateOnly,
                                                    IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods,
                                                    TimeSpan averageWorkTimePerDay, ISchedulingOptions schedulingOptions)
		{
			var wfcs = person.WorkflowControlSet;
			if (wfcs == null)
				return allValues.ToList();

			var fairnessType = wfcs.GetFairnessType(false, false);

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
		            double fairnessValue;
		            if (fairnessType == FairnessType.EqualNumberOfShiftCategory)
		            {
			            IShiftCategory shiftCategory = shiftProjection.TheMainShift.ShiftCategory;
			            double factorForShiftCategory = shiftCategoryFairnessFactors.FairnessFactor(shiftCategory);
			            fairnessValue =
				            _categoryFairnessShiftValueCalculator.ModifiedShiftValue(thisShiftValue.Value,
				                                                                     factorForShiftCategory,
				                                                                     maxValue, schedulingOptions);
		            }
		            else
		            {
			            var maxFairnessOnShiftCat = getMaxFairnessOnShiftCategories();
			            double totalFairness = shiftCategoryFairnessFactors.FairnessPointsPerShift;
			            IFairnessValueResult agentFairness = ScheduleDictionary[person].FairnessPoints();
			            int dayOfWeekJusticeValue = shiftProjection.ShiftCategoryDayOfWeekJusticeValue;
			            fairnessValue = _fairnessValueCalculator.CalculateFairnessValue(thisShiftValue.Value,
			                                                                            dayOfWeekJusticeValue,
			                                                                            maxFairnessOnShiftCat,
			                                                                            totalFairness,
			                                                                            agentFairness, maxValue,
			                                                                            schedulingOptions);
		            }

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

        protected IScheduleDictionary ScheduleDictionary
        {
            get { return _resultStateHolder().Schedules; }
        }

        protected int getMaxFairnessOnShiftCategories()
        {
            int result = 0;
            foreach (var shiftCategory in _resultStateHolder().ShiftCategories)
            {
                int temp = shiftCategory.MaxOfJusticeValues();
                if (temp > result)
                    result = temp;
            }
            return result;
        }
    }
}
