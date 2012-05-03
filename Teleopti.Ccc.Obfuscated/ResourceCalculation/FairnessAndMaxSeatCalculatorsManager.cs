using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public interface IFairnessAndMaxSeatCalculatorsManager
    {
        IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues,
                                               double maxValue, bool useShiftCategoryFairness, IPerson person, DateOnly dateOnly,
                                               IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods, TimeSpan averageWorkTimePerDay);
    }

    public class FairnessAndMaxSeatCalculatorsManager : IFairnessAndMaxSeatCalculatorsManager
    {
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        private readonly IAverageShiftLengthValueCalculator _averageShiftLengthValueCalculator;
        private readonly IShiftCategoryFairnessManager _shiftCategoryFairnessManager;
        private readonly IShiftCategoryFairnessShiftValueCalculator _categoryFairnessShiftValueCalculator;
        private readonly IFairnessValueCalculator _fairnessValueCalculator;
        private readonly ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;
        private readonly ISchedulingOptions _options;

        public FairnessAndMaxSeatCalculatorsManager(ISchedulingResultStateHolder resultStateHolder,
                                    IAverageShiftLengthValueCalculator averageShiftLengthValueCalculator,
                                    IShiftCategoryFairnessManager shiftCategoryFairnessManager,
                                    IShiftCategoryFairnessShiftValueCalculator categoryFairnessShiftValueCalculator,
                                    IFairnessValueCalculator fairnessValueCalculator,
                                    ISeatLimitationWorkShiftCalculator2 seatLimitationWorkShiftCalculator,
                                    ISchedulingOptions options)
        {
            _resultStateHolder = resultStateHolder;
            _averageShiftLengthValueCalculator = averageShiftLengthValueCalculator;
            _shiftCategoryFairnessManager = shiftCategoryFairnessManager;
            _categoryFairnessShiftValueCalculator = categoryFairnessShiftValueCalculator;
            _fairnessValueCalculator = fairnessValueCalculator;
            _seatLimitationWorkShiftCalculator = seatLimitationWorkShiftCalculator;
            _options = options;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues,
                                                    double maxValue, bool useShiftCategoryFairness, IPerson person, DateOnly dateOnly,
                                                    IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods,
                                                    TimeSpan averageWorkTimePerDay)
        {
            double highestShiftValue = double.MinValue;
            var shiftCategoryFairnessFactors = _shiftCategoryFairnessManager.GetFactorsForPersonOnDate(person, dateOnly);
            var foundValues = new List<IWorkShiftCalculationResultHolder>();


            foreach (WorkShiftCalculationResultHolder thisShiftValue in allValues)
            {
                IShiftProjectionCache shiftProjection = thisShiftValue.ShiftProjection;
                double fairnessValue;

                if (useShiftCategoryFairness)
                {
                    IShiftCategory shiftCategory = shiftProjection.TheMainShift.ShiftCategory;
                    double factorForShiftCategory = shiftCategoryFairnessFactors.FairnessFactor(shiftCategory);
                    fairnessValue =
                        _categoryFairnessShiftValueCalculator.ModifiedShiftValue(thisShiftValue.Value,
                                                                                 factorForShiftCategory,
                                                                                 maxValue);
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
                                                                                   agentFairness, maxValue);
                }

                double shiftValue = fairnessValue;

                

                if (shiftValue > highestShiftValue && _options.UseMaxSeats)
                {
                    var seatVal =
                        _seatLimitationWorkShiftCalculator.CalculateShiftValue(person,
                                                                               shiftProjection.MainShiftProjection,
                                                                               maxSeatSkillPeriods,
                                                                               _options.DoNotBreakMaxSeats);
                    if (!seatVal.HasValue)
                        continue;

                    shiftValue += seatVal.Value;
                }

				var temp = PrincipalAuthorization.Instance().IsPermitted(
					DefinedRaptorApplicationFunctionPaths.UnderConstruction);
				if(!temp)
				{
					if (_options.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff &&
						_options.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime)
					{
						shiftValue = _averageShiftLengthValueCalculator.CalculateShiftValue(shiftValue,
																						   shiftProjection.MainShiftProjection.ContractTime(),
																						   averageWorkTimePerDay);
					}
				}
				

                if (shiftValue > highestShiftValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResultHolder { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues = new List<IWorkShiftCalculationResultHolder> { workShiftFinderResultHolder };
                    highestShiftValue = shiftValue;
                    continue;
                }
                if (shiftValue == highestShiftValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResultHolder { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues.Add(workShiftFinderResultHolder);
                }
            }
            return foundValues;
        }

        private IScheduleDictionary ScheduleDictionary
        {
            get { return _resultStateHolder.Schedules; }
        }

        private int getMaxFairnessOnShiftCategories()
        {
            int result = 0;
            foreach (var shiftCategory in _resultStateHolder.ShiftCategories)
            {
                int temp = shiftCategory.MaxOfJusticeValues();
                if (temp > result)
                    result = temp;
            }
            return result;
        }
    }
}
