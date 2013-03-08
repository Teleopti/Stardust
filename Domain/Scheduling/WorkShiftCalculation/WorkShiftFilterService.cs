using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IWorkShiftFilterService
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions);
	}

	public class WorkShiftFilterService : IWorkShiftFilterService
	{
		private readonly IActivityRestrictionsShiftFilter _activityRestrictionsShiftFilter;
		private readonly IBusinessRulesShiftFilter _businessRulesShiftFilter;
		private readonly ICommonMainShiftFilter _commonMainShiftFilter;
		private readonly IContractTimeShiftFilter _contractTimeShiftFilter;
		private readonly IDisallowedShiftCategoriesShiftFilter _disallowedShiftCategoriesShiftFilter;
		private readonly IEffectiveRestrictionShiftFilter _effectiveRestrictionShiftFilter;
		private readonly IMainShiftOptimizeActivitiesSpecificationShiftFilter _mainShiftOptimizeActivitiesSpecificationShiftFilter;
		private readonly INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;
		private readonly IPersonalShiftsShiftFilter _personalShiftsShiftFilter;
		private readonly IShiftCategoryRestrictionShiftFilter _shiftCategoryRestrictionShiftFilter;
		private readonly IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter;
		private readonly ITimeLimitsRestrictionShiftFilter _timeLimitsRestrictionShiftFilter;
		private readonly IWorkTimeLimitationShiftFilter _workTimeLimitationShiftFilter;
		private readonly IShiftLengthDecider _shiftLengthDecider;
		private readonly IWorkShiftMinMaxCalculator _minMaxCalculator;

		public WorkShiftFilterService(IActivityRestrictionsShiftFilter activityRestrictionsShiftFilter,
			IBusinessRulesShiftFilter businessRulesShiftFilter,
			ICommonMainShiftFilter commonMainShiftFilter,
			IContractTimeShiftFilter contractTimeShiftFilter,
			IDisallowedShiftCategoriesShiftFilter disallowedShiftCategoriesShiftFilter,
			IEffectiveRestrictionShiftFilter effectiveRestrictionShiftFilter,
			IMainShiftOptimizeActivitiesSpecificationShiftFilter mainShiftOptimizeActivitiesSpecificationShiftFilter,
			INotOverWritableActivitiesShiftFilter notOverWritableActivitiesShiftFilter,
			IPersonalShiftsShiftFilter personalShiftsShiftFilter,
			IShiftCategoryRestrictionShiftFilter shiftCategoryRestrictionShiftFilter,
			IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter,
			ITimeLimitsRestrictionShiftFilter timeLimitsRestrictionShiftFilter,
			IWorkTimeLimitationShiftFilter workTimeLimitationShiftFilter,
			IShiftLengthDecider shiftLengthDecider,
			IWorkShiftMinMaxCalculator minMaxCalculator)
		{
			_activityRestrictionsShiftFilter = activityRestrictionsShiftFilter;
			_businessRulesShiftFilter = businessRulesShiftFilter;
			_commonMainShiftFilter = commonMainShiftFilter;
			_contractTimeShiftFilter = contractTimeShiftFilter;
			_disallowedShiftCategoriesShiftFilter = disallowedShiftCategoriesShiftFilter;
			_effectiveRestrictionShiftFilter = effectiveRestrictionShiftFilter;
			_mainShiftOptimizeActivitiesSpecificationShiftFilter = mainShiftOptimizeActivitiesSpecificationShiftFilter;
			_notOverWritableActivitiesShiftFilter = notOverWritableActivitiesShiftFilter;
			_personalShiftsShiftFilter = personalShiftsShiftFilter;
			_shiftCategoryRestrictionShiftFilter = shiftCategoryRestrictionShiftFilter;
			_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter = shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter;
			_timeLimitsRestrictionShiftFilter = timeLimitsRestrictionShiftFilter;
			_workTimeLimitationShiftFilter = workTimeLimitationShiftFilter;
			_shiftLengthDecider = shiftLengthDecider;
			_minMaxCalculator = minMaxCalculator;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo ,
													IEffectiveRestriction effectiveRestriction,
													ISchedulingOptions schedulingOptions)
		{
		    var person = teamBlockInfo.TeamInfo.GroupPerson;
		    var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
            FinderResult = new WorkShiftFinderResult(person, dateOnly);
			if (effectiveRestriction == null)
				return null;
			if (person == null) return null;
			var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction, FinderResult))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var shiftList = _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(dateOnly, person, false,
																						   effectiveRestriction);
			shiftList = _commonMainShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(shiftList,
																					schedulingOptions
																						.MainShiftOptimizeActivitySpecification);

			shiftList = _shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, shiftList, FinderResult);
			shiftList = _disallowedShiftCategoriesShiftFilter.Filter(schedulingOptions.NotAllowedShiftCategories, shiftList,
																	 FinderResult);
			shiftList = _activityRestrictionsShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction, FinderResult);
			shiftList = _timeLimitsRestrictionShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction, FinderResult);
			shiftList = _workTimeLimitationShiftFilter.Filter(shiftList, effectiveRestriction, FinderResult);

			shiftList = _contractTimeShiftFilter.Filter(dateOnly, matrixList, shiftList, schedulingOptions, FinderResult);
			shiftList = _businessRulesShiftFilter.Filter(person, shiftList, dateOnly, FinderResult);
			shiftList = _notOverWritableActivitiesShiftFilter.Filter(dateOnly, person, shiftList, FinderResult);
			shiftList = _personalShiftsShiftFilter.Filter(dateOnly, person, shiftList, FinderResult);

			if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime && matrixList != null)
			{
				shiftList = _shiftLengthDecider.FilterList(shiftList, _minMaxCalculator, matrixList[0], schedulingOptions);
			}

			return shiftList.Count == 0 ? null : shiftList;
		}

		public IWorkShiftFinderResult FinderResult { get; private set; }
	}
}