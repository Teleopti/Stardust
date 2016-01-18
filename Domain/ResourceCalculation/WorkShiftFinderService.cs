using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class WorkShiftFinderService : IWorkShiftFinderService
    {
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly Func<IPersonSkillPeriodsDataHolderManager> _personSkillPeriodsDataHolderManager;
        private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private readonly IWorkShiftCalculatorsManager _workShiftCalculatorsManager;
        private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
        private readonly Func<IPreSchedulingStatusChecker> _preSchedulingStatusChecker;
        private readonly Func<IWorkShiftMinMaxCalculator> _workShiftMinMaxCalculator;
        private readonly IFairnessAndMaxSeatCalculatorsManager _fairnessAndMaxSeatCalculatorsManager;
    	private readonly IShiftLengthDecider _shiftLengthDecider;
	    private readonly IPersonSkillDayCreator _personSkillDayCreator;

	    public WorkShiftFinderService(Func<ISchedulingResultStateHolder> resultStateHolder, Func<IPreSchedulingStatusChecker> preSchedulingStatusChecker
            , IShiftProjectionCacheFilter shiftProjectionCacheFilter, Func<IPersonSkillPeriodsDataHolderManager> personSkillPeriodsDataHolderManager,  
            IShiftProjectionCacheManager shiftProjectionCacheManager ,  IWorkShiftCalculatorsManager workShiftCalculatorsManager,  
            Func<IWorkShiftMinMaxCalculator> workShiftMinMaxCalculator, IFairnessAndMaxSeatCalculatorsManager fairnessAndMaxSeatCalculatorsManager,
			IShiftLengthDecider shiftLengthDecider, IPersonSkillDayCreator personSkillDayCreator)
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
        }

        public WorkShiftFinderServiceResult FindBestShift(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction)
        {
            _workShiftMinMaxCalculator().ResetCache();
			var scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
            var person = schedulePart.Person;
			var finderResult = new WorkShiftFinderResult(person, scheduleDateOnly);
            var status = _preSchedulingStatusChecker().CheckStatus(schedulePart, finderResult, schedulingOptions);
            
            if (!status) return new WorkShiftFinderServiceResult(null,finderResult);

            IVirtualSchedulePeriod currentSchedulePeriod = person.VirtualSchedulePeriod(scheduleDateOnly);

            if (!currentSchedulePeriod.IsValid)
				return new WorkShiftFinderServiceResult(null, finderResult);

            if (!_shiftProjectionCacheFilter.CheckRestrictions(schedulingOptions, effectiveRestriction, finderResult))
				return new WorkShiftFinderServiceResult(null, finderResult);

            var timeZone = person.PermissionInformation.DefaultTimeZone();

            var personPeriod = person.Period(scheduleDateOnly);
            IRuleSetBag bag = personPeriod.RuleSetBag;

            var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(scheduleDateOnly, timeZone, bag, false, true);
			
            IWorkShiftCalculationResultHolder result = null;
	        if (shiftList.Count > 0)
	        {
		        result = findBestShift(effectiveRestriction, currentSchedulePeriod, scheduleDateOnly, person, matrix, schedulingOptions, finderResult,shiftList);
	        }
            else
            {
	            finderResult.AddFilterResults(new WorkShiftFilterResult(Resources.NoShiftsFound, 0 ,0));
            }

			if (result == null && (schedulingOptions.UsePreferences || schedulingOptions.UseAvailability || schedulingOptions.UseRotations || schedulingOptions.UseStudentAvailability))
			{
				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(scheduleDateOnly, timeZone, bag, true, true);
				if (shiftList.Count > 0)
					result = findBestShift(effectiveRestriction, currentSchedulePeriod, scheduleDateOnly, person, matrix, schedulingOptions, finderResult,shiftList);
			}

            return new WorkShiftFinderServiceResult(result,finderResult);
        }

        private IWorkShiftCalculationResultHolder findHighestValueMainShift(
            DateOnly dateOnly,  
            IList<IShiftProjectionCache> shiftProjectionCaches,
			IWorkShiftCalculatorSkillStaffPeriodData dataHolders, 
            IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods, 
            IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, 
            IVirtualSchedulePeriod currentSchedulePeriod,
            ISchedulingOptions schedulingOptions,
			IWorkShiftFinderResult workShiftFinderResult
		)
        {
			var person = currentSchedulePeriod.Person;
        	IList<IWorkShiftCalculationResultHolder> allValues = _workShiftCalculatorsManager.RunCalculators(person,
        	                                                                                                 shiftProjectionCaches,
																											 dataHolders, 
        	                                                                                                 nonBlendSkillPeriods,
        	                                                                                                 schedulingOptions);
            if (allValues.Count == 0)
                return null;

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            foreach (var workShiftCalculationResultHolder in allValues)
            {
                if (workShiftCalculationResultHolder.Value > maxValue)
                    maxValue = workShiftCalculationResultHolder.Value;
                if (workShiftCalculationResultHolder.Value < minValue)
                    minValue = workShiftCalculationResultHolder.Value;
            }

        	var foundValues =
        		_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(allValues, maxValue,
        		                                                             person, dateOnly, maxSeatSkillPeriods,
        		                                                             currentSchedulePeriod.AverageWorkTimePerDay,
        		                                                             schedulingOptions).ToArray();
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
					workShiftFinderResult.StoppedOnOverstaffing = true;
					workShiftFinderResult.Successful = true;
					return null;
				}
			}

			foundValues = WorkShiftCalculator.CalculateListForBestImprovementAfterAssignment(foundValues, dataHolders).OfType<IWorkShiftCalculationResultHolder>().ToArray();
	        int foundValuesCount = foundValues.Length;
	        if (foundValuesCount == 1)
		        return foundValues[0];

			IDictionary<int, int> randomResultList = new Dictionary<int, int>(foundValuesCount);
			for (int i = 0; i < foundValuesCount; i++)
	        {
		        randomResultList.Add(i, 0);
	        }

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


	    private void loggFilterResult(string message, int countWorkShiftsBefore, int countWorkShiftsAfter, IWorkShiftFinderResult workShiftFinderResult)
        {
            workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(message, countWorkShiftsBefore, countWorkShiftsAfter));
        }

		private IWorkShiftCalculationResultHolder findBestShift(IEffectiveRestriction effectiveRestriction,
            IVirtualSchedulePeriod virtualSchedulePeriod, DateOnly dateOnly, IPerson person, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, IWorkShiftFinderResult workShiftFinderResult, IList<IShiftProjectionCache> shiftList)
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
				                                                                                         	NotAllowedShiftCategories,
				                                                                                         workShiftFinderResult);


				if (shiftList.Count == 0)
					return null;

				MinMax<TimeSpan>? allowedMinMax = _workShiftMinMaxCalculator().MinMaxAllowedShiftContractTime(dateOnly, matrix,
				                                                                                            schedulingOptions);

				if (!allowedMinMax.HasValue)
				{
					loggFilterResult(Resources.NoShiftsThatMatchesTheContractTimeCouldBeFound, shiftList.Count, 0, workShiftFinderResult);
					return null;
				}

				IScheduleRange wholeRange = _resultStateHolder().Schedules[person];
				shiftList = _shiftProjectionCacheFilter.Filter(allowedMinMax.Value, shiftList, dateOnly, wholeRange,
				                                                workShiftFinderResult);
				if (shiftList.Count == 0)
					return null;

				if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime)
				{
					shiftList = _shiftLengthDecider.FilterList(shiftList, _workShiftMinMaxCalculator(), matrix, schedulingOptions);
					if (shiftList.Count == 0)
						return null;
				}

			}

			IWorkShiftCalculationResultHolder result;
            using (PerformanceOutput.ForOperation("Calculating and selecting best shift"))
            {
	            var personSkillPeriodsDataHolderManager = _personSkillPeriodsDataHolderManager();
	            var personSkillDay = _personSkillDayCreator.Create(dateOnly, virtualSchedulePeriod);
	            var maxSeatPeriods = personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(personSkillDay);
                var nonBlendPeriods = personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(personSkillDay);
                var dataholder = personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(personSkillDay);

				result = findHighestValueMainShift(dateOnly, shiftList, new WorkShiftCalculatorSkillStaffPeriodData(dataholder), maxSeatPeriods, nonBlendPeriods, virtualSchedulePeriod, schedulingOptions, workShiftFinderResult);
            }

            return result;
        }
    }
}
