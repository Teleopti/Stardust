using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class WorkShiftFinderService
    {
        private readonly ShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly Func<IPersonSkillPeriodsDataHolderManager> _personSkillPeriodsDataHolderManager;
        private readonly ShiftProjectionCacheManager _shiftProjectionCacheManager;
        private readonly WorkShiftCalculatorsManager _workShiftCalculatorsManager;
        private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
        private readonly Func<PreSchedulingStatusChecker> _preSchedulingStatusChecker;
        private readonly Func<IWorkShiftMinMaxCalculator> _workShiftMinMaxCalculator;
        private readonly FairnessAndMaxSeatCalculatorsManager28317 _fairnessAndMaxSeatCalculatorsManager;
    	private readonly IShiftLengthDecider _shiftLengthDecider;
	    private readonly PersonSkillDayCreator _personSkillDayCreator;
		private readonly IOpenHoursSkillExtractor _openHoursSkillExtractor;

		public WorkShiftFinderService(Func<ISchedulingResultStateHolder> resultStateHolder, Func<PreSchedulingStatusChecker> preSchedulingStatusChecker
            ,ShiftProjectionCacheFilter shiftProjectionCacheFilter, Func<IPersonSkillPeriodsDataHolderManager> personSkillPeriodsDataHolderManager,  
			ShiftProjectionCacheManager shiftProjectionCacheManager ,  WorkShiftCalculatorsManager workShiftCalculatorsManager,  
            Func<IWorkShiftMinMaxCalculator> workShiftMinMaxCalculator, FairnessAndMaxSeatCalculatorsManager28317 fairnessAndMaxSeatCalculatorsManager,
			IShiftLengthDecider shiftLengthDecider, PersonSkillDayCreator personSkillDayCreator, IOpenHoursSkillExtractor openHoursSkillExtractor)
        {
            _resultStateHolder = resultStateHolder;
            _preSchedulingStatusChecker = preSchedulingStatusChecker;
            _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
            _personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
            _shiftProjectionCacheManager = shiftProjectionCacheManager;
            _workShiftCalculatorsManager = workShiftCalculatorsManager;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _fairnessAndMaxSeatCalculatorsManager = fairnessAndMaxSeatCalculatorsManager;
        	_shiftLengthDecider = shiftLengthDecider;
		    _personSkillDayCreator = personSkillDayCreator;
			_openHoursSkillExtractor = openHoursSkillExtractor;
		}

        public IWorkShiftCalculationResultHolder FindBestShift(IScheduleDay schedulePart, SchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction)
        {
            _workShiftMinMaxCalculator().ResetCache();
			var scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
            var person = schedulePart.Person;
            var status = _preSchedulingStatusChecker().CheckStatus(schedulePart, schedulingOptions);
            
            if (!status) return null;

            IVirtualSchedulePeriod currentSchedulePeriod = person.VirtualSchedulePeriod(scheduleDateOnly);

            if (!currentSchedulePeriod.IsValid)
				return null;

			if (!_shiftProjectionCacheFilter.CheckRestrictions(schedulingOptions, effectiveRestriction))
				return null;

			var personPeriod = person.Period(scheduleDateOnly);
            IRuleSetBag bag = personPeriod.RuleSetBag;

            var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(schedulePart.DateOnlyAsPeriod, bag, false, true);
	        
            IWorkShiftCalculationResultHolder result = null;
	        if (shiftList.Count > 0)
	        {
				result = findBestShift(effectiveRestriction, currentSchedulePeriod, scheduleDateOnly, person, matrix, schedulingOptions, shiftList);
	        }

			if (result == null && (schedulingOptions.UsePreferences || schedulingOptions.UseAvailability || schedulingOptions.UseRotations || schedulingOptions.UseStudentAvailability))
			{
				if (!effectiveRestriction.IsRestriction)
					return null;

				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(schedulePart.DateOnlyAsPeriod, bag, true,
					true);
				if (shiftList.Count > 0)
				{
					result = findBestShift(effectiveRestriction, currentSchedulePeriod, scheduleDateOnly, person, matrix,
						schedulingOptions, shiftList);
				}
			}

            return result;
        }

        private IWorkShiftCalculationResultHolder findHighestValueMainShift(
            IList<ShiftProjectionCache> shiftProjectionCaches,
			IWorkShiftCalculatorSkillStaffPeriodData dataHolders, 
            IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, 
            IVirtualSchedulePeriod currentSchedulePeriod,
            SchedulingOptions schedulingOptions
		)
        {
			var person = currentSchedulePeriod.Person;
        	var allValues = _workShiftCalculatorsManager.RunCalculators(person,
        	                                                                                                 shiftProjectionCaches,
																											 dataHolders, 
        	                                                                                                 nonBlendSkillPeriods,
        	                                                                                                 schedulingOptions);
            if (!allValues.Any())
                return null;

			IWorkShiftCalculationResultHolder[] foundValues = { };
			if (schedulingOptions.WorkShiftLengthHintOption != WorkShiftLengthHintOption.Long)
			{
				foundValues = _fairnessAndMaxSeatCalculatorsManager.FindBestShiftAccordingToValue(allValues).ToArray();
			}
			else
			{
				foundValues = _fairnessAndMaxSeatCalculatorsManager.FindBestLongShiftAccordingToValue(allValues).ToArray();
			}

			if (foundValues.IsEmpty())
		        return null;

			// if we only want shifts that don't overstaff
			if (schedulingOptions.OnlyShiftsWhenUnderstaffed)
			{
				double highestShiftValue = double.MinValue;
				foreach (var workShiftCalculationResultHolder in foundValues)
				{
					if (workShiftCalculationResultHolder.Value > highestShiftValue)
						highestShiftValue = workShiftCalculationResultHolder.Value;
				}

				if (Math.Abs(highestShiftValue - double.MinValue) < 0.00001)
					return null;

				if (highestShiftValue <= 0)
				{
					return null;
				}
			}

			foundValues = NonSecretWorkShiftCalculatorClassic.CalculateListForBestImprovementAfterAssignment(foundValues, dataHolders).OfType<IWorkShiftCalculationResultHolder>().ToArray();
	        int foundValuesCount = foundValues.Length;
	        if (foundValuesCount == 1)
		        return foundValues[0];

	        var randomResultList = Enumerable.Range(0, foundValuesCount).ToDictionary(k => k, v => 0);
	        for (int i = 0; i < 100; i++)
	        {
				var rndResult = foundValues.GetRandom();
		        var index = Array.IndexOf(foundValues,rndResult);
		        randomResultList[index] = randomResultList[index] + 1;
	        }

	        int winningIndexValue = randomResultList.Values.Max();

	        foreach (var keyValuePair in randomResultList)
	        {
		        if (keyValuePair.Value == winningIndexValue)
			        return foundValues[keyValuePair.Key];
	        }

	        return null;
        }

		private IWorkShiftCalculationResultHolder findBestShift(IEffectiveRestriction effectiveRestriction,
            IVirtualSchedulePeriod virtualSchedulePeriod, DateOnly dateOnly, IPerson person, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, IList<ShiftProjectionCache> shiftList)
        {
			using (PerformanceOutput.ForOperation("Filtering shifts before calculating"))
			{
				if (schedulingOptions.ShiftCategory != null)
				{
					// override the one in Effective
					effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;
				}

			    shiftList = _shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(shiftList, schedulingOptions.MainShiftOptimizeActivitySpecification);

				shiftList = _shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(dateOnly,
				                                                                                         person.
				                                                                                         	PermissionInformation.
				                                                                                         	DefaultTimeZone(),
				                                                                                         shiftList,
				                                                                                         effectiveRestriction,
				                                                                                         schedulingOptions.
				                                                                                         	NotAllowedShiftCategories);


				if (shiftList.Count == 0)
					return null;

				MinMax<TimeSpan>? allowedMinMax = _workShiftMinMaxCalculator().MinMaxAllowedShiftContractTime(dateOnly, matrix,
				                                                                                            schedulingOptions);

				if (!allowedMinMax.HasValue)
				{
					return null;
				}

				IScheduleRange wholeRange = _resultStateHolder().Schedules[person];
				shiftList = _shiftProjectionCacheFilter.Filter(_resultStateHolder().Schedules, allowedMinMax.Value, shiftList, dateOnly, wholeRange);
				if (shiftList.Count == 0)
					return null;

				if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime)
				{
					var group = new Group(new List<IPerson> {person}, string.Empty);
					var teamInfo = new TeamInfo(group, new List<IList<IScheduleMatrixPro>>());
					var blockInfo = new BlockInfo(dateOnly.ToDateOnlyPeriod());
					var teamBlockInfo = new TeamBlockInfo(teamInfo, blockInfo);
					var openHoursResult = _openHoursSkillExtractor.Extract(teamBlockInfo, _resultStateHolder().SkillDays.ToSkillDayEnumerable(), new DateOnlyPeriod(matrix.FullWeeksPeriodDays.Min(x => x.Day), matrix.FullWeeksPeriodDays.Max(x => x.Day)));
					shiftList = _shiftLengthDecider.FilterList(shiftList, _workShiftMinMaxCalculator(), matrix, schedulingOptions, openHoursResult, dateOnly);
					if (shiftList.Count == 0)
						return null;
				}
			}

			IWorkShiftCalculationResultHolder result;
            using (PerformanceOutput.ForOperation("Calculating and selecting best shift"))
            {
	            var personSkillPeriodsDataHolderManager = _personSkillPeriodsDataHolderManager();
	            var personSkillDay = _personSkillDayCreator.Create(dateOnly, virtualSchedulePeriod);
                var nonBlendPeriods = personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(personSkillDay);
                var dataholder = personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(personSkillDay);

				result = findHighestValueMainShift(shiftList, new WorkShiftCalculatorSkillStaffPeriodData(dataholder), nonBlendPeriods, virtualSchedulePeriod, schedulingOptions);
            }

            return result;
        }
    }
}
