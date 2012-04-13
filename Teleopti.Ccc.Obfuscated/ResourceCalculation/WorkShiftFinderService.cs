using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class WorkShiftFinderService : IWorkShiftFinderService
    {
        private DateOnly _scheduleDateOnly;
        private IPerson _person;
        
        private IWorkShiftFinderResult _finderResult;
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
        private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private readonly IWorkShiftCalculatorsManager _workShiftCalculatorsManager;
        private IList<IShiftProjectionCache> _shiftList;
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        private readonly IPreSchedulingStatusChecker _preSchedulingStatusChecker;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly IFairnessAndMaxSeatCalculatorsManager _fairnessAndMaxSeatCalculatorsManager;
        private readonly ISchedulingOptions _schedulingOptions;

        public WorkShiftFinderService(ISchedulingResultStateHolder resultStateHolder, IPreSchedulingStatusChecker preSchedulingStatusChecker
            , IShiftProjectionCacheFilter shiftProjectionCacheFilter, IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager,  
            IShiftProjectionCacheManager shiftProjectionCacheManager ,  IWorkShiftCalculatorsManager workShiftCalculatorsManager,  
            IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IFairnessAndMaxSeatCalculatorsManager fairnessAndMaxSeatCalculatorsManager,
            ISchedulingOptions schedulingOptions)
        {
            _resultStateHolder = resultStateHolder;
            _preSchedulingStatusChecker = preSchedulingStatusChecker;
            _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
            _personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
            _shiftProjectionCacheManager = shiftProjectionCacheManager;
            _workShiftCalculatorsManager = workShiftCalculatorsManager;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _fairnessAndMaxSeatCalculatorsManager = fairnessAndMaxSeatCalculatorsManager;
            _schedulingOptions = schedulingOptions;
        }

        private IScheduleDictionary ScheduleDictionary
        {
            get { return _resultStateHolder.Schedules; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IWorkShiftCalculationResultHolder FindBestShift(IScheduleDay schedulePart,IEffectiveRestriction effectiveRestriction, IScheduleMatrixPro matrix)
        {
            _workShiftMinMaxCalculator.ResetCache();
            _scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
            _person = schedulePart.Person;
            _finderResult = null;
            var status = _preSchedulingStatusChecker.CheckStatus(schedulePart, FinderResult);
                        
            
            if (!status)
                return null;

            IVirtualSchedulePeriod currentSchedulePeriod = _person.VirtualSchedulePeriod(_scheduleDateOnly);

            if (!currentSchedulePeriod.IsValid)
                return null;

            if (!_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, FinderResult))
                return null;

            var timeZone = _person.PermissionInformation.DefaultTimeZone();

            var personPeriod = _person.Period(_scheduleDateOnly);
            IRuleSetBag bag = personPeriod.RuleSetBag;

            _shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_scheduleDateOnly, timeZone, bag, false);

            IWorkShiftCalculationResultHolder result = null;
            if(_shiftList.Count > 0)
                result = findBestShift(effectiveRestriction, currentSchedulePeriod, _scheduleDateOnly, _person, matrix);

            if(result == null && effectiveRestriction.IsRestriction)
            {
                _shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_scheduleDateOnly, timeZone, bag, true);
                if (_shiftList == null || _shiftList.Count == 0)
                {
                    loggFilterResult(UserTexts.Resources.NoWorkShiftsWereAvailable, 0, 0);
                    return null;
                }
                result = findBestShift(effectiveRestriction, currentSchedulePeriod, _scheduleDateOnly, _person, matrix);
            }
            return result;
        }

        public IWorkShiftFinderResult FinderResult
        {
            get
            {
                if (_finderResult != null) return _finderResult;
                _finderResult = new WorkShiftFinderResult(_person, _scheduleDateOnly);
                return _finderResult;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
        public IWorkShiftCalculationResultHolder FindBestMainShift(
            DateOnly dateOnly,  
            IList<IShiftProjectionCache> shiftProjectionCaches,
			IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, 
            IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods, 
            IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, 
            IVirtualSchedulePeriod currentSchedulePeriod)
        {
			var person = currentSchedulePeriod.Person;
            IList<IWorkShiftCalculationResultHolder> allValues = _workShiftCalculatorsManager.RunCalculators(person, shiftProjectionCaches, dataHolders, nonBlendSkillPeriods);
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

            bool useShiftCategoryFairness = false;
            if (person.WorkflowControlSet != null)
                useShiftCategoryFairness = person.WorkflowControlSet.UseShiftCategoryFairness;

           IList<IWorkShiftCalculationResultHolder> foundValues =
               _fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(allValues, maxValue, useShiftCategoryFairness, person, dateOnly,  maxSeatSkillPeriods, currentSchedulePeriod.AverageWorkTimePerDay);
           
            double highestShiftValue = double.MinValue;
            foreach (var workShiftCalculationResultHolder in foundValues)
            {
                if (workShiftCalculationResultHolder.Value > highestShiftValue)
                    highestShiftValue = workShiftCalculationResultHolder.Value;
            }

		    if (highestShiftValue == double.MinValue)
				return null;

			if (foundValues.Count == 0)
				return null;

			// if we only want shifts that don't overstaff
            if (_schedulingOptions.OnlyShiftsWhenUnderstaffed && highestShiftValue <= 0)
			{
				FinderResult.StoppedOnOverstaffing = true;
				FinderResult.Successful = true;
				return null;
			}
            
             foundValues = WorkShiftCalculator.CalculateListForBestImprovementAfterAssignment(foundValues, dataHolders);
            return foundValues.GetRandom();
		    
		}


        #region Private methods
        private void loggFilterResult(string message, int countWorkShiftsBefore, int countWorkShiftsAfter)
        {
            FinderResult.AddFilterResults(new WorkShiftFilterResult(message, countWorkShiftsBefore, countWorkShiftsAfter));
        }

        private IWorkShiftCalculationResultHolder findBestShift(IEffectiveRestriction effectiveRestriction,
            IVirtualSchedulePeriod virtualSchedulePeriod, DateOnly dateOnly, IPerson person, IScheduleMatrixPro matrix)
        {
            using (PerformanceOutput.ForOperation("Filtering shifts before calculating"))
            {
                if (_schedulingOptions.ShiftCategory != null)
                {
                    // override the one in Effective
                    effectiveRestriction.ShiftCategory = _schedulingOptions.ShiftCategory;
                }

                _shiftList = _shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(_shiftList);

                _shiftList = _shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(dateOnly,
                                                                                                         person.PermissionInformation.DefaultTimeZone(),
                                                                                                         _shiftList,
                                                                                                         effectiveRestriction,
                                                                                                         _schedulingOptions.NotAllowedShiftCategories,
                                                                                                          FinderResult);
                if (_shiftList.Count == 0)
                    return null;

            	MinMax<TimeSpan>? allowedMinMax = _workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, matrix);

                if (!allowedMinMax.HasValue)
                {
                    loggFilterResult(UserTexts.Resources.NoShiftsThatMatchesTheContractTimeCouldBeFound, _shiftList.Count, 0);
                    return null;
                }

                IScheduleRange wholeRange = ScheduleDictionary[person];
                _shiftList = _shiftProjectionCacheFilter.Filter(allowedMinMax.Value, _shiftList, _scheduleDateOnly, wholeRange, FinderResult);
                if (_shiftList.Count == 0)
                    return null;

                if (_schedulingOptions.RescheduleOptions == OptimizationRestriction.KeepStartAndEndTime && _schedulingOptions.SpecificStartAndEndTime.HasValue)
                {
                    _shiftList = _shiftProjectionCacheFilter.FilterOnStartAndEndTime(_schedulingOptions.SpecificStartAndEndTime.Value, _shiftList, _finderResult);
                    if (_shiftList.Count == 0)
                        return null;
                }
            }

            IWorkShiftCalculationResultHolder result;
            using (PerformanceOutput.ForOperation("Calculating and selecting best shift"))
            {
                var maxSeatPeriods = _personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(dateOnly, virtualSchedulePeriod);
                var nonBlendPeriods = _personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(dateOnly, virtualSchedulePeriod);
                var dataholder = _personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(dateOnly, virtualSchedulePeriod);

                result = FindBestMainShift(dateOnly, _shiftList, dataholder,maxSeatPeriods, nonBlendPeriods, virtualSchedulePeriod);
            }

            return result;
        }

        #endregion
    }
}
