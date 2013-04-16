using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IWorkShiftFilterService
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult);
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
													ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
		    var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly).ToList();
			if (matrixList.Count == 0) return null;
			var currentSchedulePeriod = groupPerson.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction, finderResult))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var shiftList = _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(dateOnly, groupPerson, false);
			shiftList = _commonMainShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(shiftList,
																					schedulingOptions
																						.MainShiftOptimizeActivitySpecification);

			shiftList = _shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, shiftList, finderResult);
			shiftList = _disallowedShiftCategoriesShiftFilter.Filter(schedulingOptions.NotAllowedShiftCategories, shiftList,
																	 finderResult);
			shiftList = _activityRestrictionsShiftFilter.Filter(dateOnly, groupPerson, shiftList, effectiveRestriction, finderResult);
			shiftList = _timeLimitsRestrictionShiftFilter.Filter(dateOnly, groupPerson, shiftList, effectiveRestriction, finderResult);
			shiftList = _workTimeLimitationShiftFilter.Filter(shiftList, effectiveRestriction, finderResult);

			shiftList = _contractTimeShiftFilter.Filter(dateOnly, matrixList, shiftList, schedulingOptions, finderResult);
			shiftList = _businessRulesShiftFilter.Filter(groupPerson, shiftList, dateOnly, finderResult);
			shiftList = _notOverWritableActivitiesShiftFilter.Filter(dateOnly, groupPerson, shiftList, finderResult);
			shiftList = _personalShiftsShiftFilter.Filter(dateOnly, groupPerson, shiftList, finderResult);

			if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime)
			{
				shiftList = _shiftLengthDecider.FilterList(shiftList, _minMaxCalculator, matrixList[0], schedulingOptions);
			}

			return shiftList.Count == 0 ? null : shiftList;
		}

	}
}