using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IWorkShiftFilterService
	{
		IList<IShiftProjectionCache> FilterForRoleModel(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult, bool options);
		IList<IShiftProjectionCache> FilterForTeamMember(DateOnly dateOnly, IPerson person, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult);
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
		private readonly ITimeLimitsRestrictionShiftFilter _timeLimitsRestrictionShiftFilter;
		private readonly IWorkTimeLimitationShiftFilter _workTimeLimitationShiftFilter;
		private readonly IShiftLengthDecider _shiftLengthDecider;
		private readonly IWorkShiftMinMaxCalculator _minMaxCalculator;
		private readonly ICommonActivityFilter _commonActivityFilter;
        private readonly IRuleSetAccordingToAccessabilityFilter _ruleSetAccordingToAccessabilityFilter;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IRuleSetPersonalSkillsActivityFilter _ruleSetPersonalSkillsActivityFilter;

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
			ITimeLimitsRestrictionShiftFilter timeLimitsRestrictionShiftFilter,
			IWorkTimeLimitationShiftFilter workTimeLimitationShiftFilter,
			IShiftLengthDecider shiftLengthDecider,
			IWorkShiftMinMaxCalculator minMaxCalculator,
			ICommonActivityFilter commonActivityFilter, 
			IRuleSetAccordingToAccessabilityFilter ruleSetAccordingToAccessabilityFilter,
			IShiftProjectionCacheManager shiftProjectionCacheManager,
			IRuleSetPersonalSkillsActivityFilter ruleSetPersonalSkillsActivityFilter)
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
			_timeLimitsRestrictionShiftFilter = timeLimitsRestrictionShiftFilter;
			_workTimeLimitationShiftFilter = workTimeLimitationShiftFilter;
			_shiftLengthDecider = shiftLengthDecider;
			_minMaxCalculator = minMaxCalculator;
			_commonActivityFilter = commonActivityFilter;
	        _ruleSetAccordingToAccessabilityFilter = ruleSetAccordingToAccessabilityFilter;
		    _shiftProjectionCacheManager = shiftProjectionCacheManager;
			_ruleSetPersonalSkillsActivityFilter = ruleSetPersonalSkillsActivityFilter;
		}

		public IList<IShiftProjectionCache> FilterForRoleModel(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
			IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult,
			bool isSameOpenHoursInBlock)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;
			var person = groupMembers.First();
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly).ToList();
			if (matrixList.Count == 0) return null;
			var firstMember = teamBlockInfo.TeamInfo.GroupMembers.First();
			var currentSchedulePeriod = firstMember.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction, finderResult))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForRoleModel(teamBlockInfo).ToList();
			var ruleSetBag = new RuleSetBag();
			foreach (var workShiftRuleSet in filteredRuleSetList)
			{
				ruleSetBag.AddRuleSet(workShiftRuleSet);
			}

			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly,
				person.PermissionInformation.DefaultTimeZone(), ruleSetBag, false, false);
			shiftList = runFiltersForRoleModel(dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList,
				person, matrixList, isSameOpenHoursInBlock);
			shiftList = runFiltersForRoleModel2(shiftList, teamBlockInfo, finderResult);

			if (allowanceToUseBlackList(shiftList, schedulingOptions, effectiveRestriction))
			{
				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly,
						person.PermissionInformation.DefaultTimeZone(), ruleSetBag, true, false);
				shiftList = runFiltersForRoleModel(dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList,
						person, matrixList, isSameOpenHoursInBlock);
				shiftList = runFiltersForRoleModel2(shiftList, teamBlockInfo, finderResult);
			}

			if (shiftList == null)
				return null;
			return shiftList.Count == 0 ? null : shiftList;
		}

		public IList<IShiftProjectionCache> FilterForTeamMember(DateOnly dateOnly, IPerson person,
			ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions,
			IWorkShiftFinderResult finderResult)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
			var matrixList =
				teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, new DateOnlyPeriod(dateOnly, dateOnly)).ToList();
			if (matrixList.Count == 0) return null;
			var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction, finderResult))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForTeamMember(person, dateOnly);
			filteredRuleSetList = _ruleSetPersonalSkillsActivityFilter.Filter(filteredRuleSetList, person, dateOnly);

			var ruleSetBag = new RuleSetBag();
			foreach (var workShiftRuleSet in filteredRuleSetList)
			{
				ruleSetBag.AddRuleSet(workShiftRuleSet);
			}

			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly,
				person.PermissionInformation.DefaultTimeZone(), ruleSetBag, false, false);

			shiftList = runFilters(dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person, matrixList,
				true);

			if (allowanceToUseBlackList(shiftList, schedulingOptions, effectiveRestriction))
			{
				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly,
					person.PermissionInformation.DefaultTimeZone(), ruleSetBag, true, false);
				shiftList = runFilters(dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person,
					matrixList, true);
			}

			if (shiftList == null)
				return null;
			return shiftList.Count == 0 ? null : shiftList;
		}

		private bool allowanceToUseBlackList(IList<IShiftProjectionCache> shiftList, ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction)
		{
			if (shiftList == null || shiftList.Count == 0)
			{
				bool useRestrictions = schedulingOptions.UsePreferences || schedulingOptions.UseAvailability ||
				                       schedulingOptions.UseRotations ||
				                       schedulingOptions.UseStudentAvailability;

				if (useRestrictions && effectiveRestriction.IsRestriction)
					return true;
			}

			return false;
		}

		private IList<IShiftProjectionCache> runFiltersForRoleModel(DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
														ISchedulingOptions schedulingOptions,
														IWorkShiftFinderResult finderResult,
														IList<IShiftProjectionCache> shiftList,
														IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameOpenHours)
		{
            if (sameOpenHours)
            {
                shiftList = _contractTimeShiftFilter.Filter(dateOnly, matrixList, shiftList, schedulingOptions, finderResult);
                shiftList = _shiftLengthDecider.FilterList(shiftList, _minMaxCalculator, matrixList[0], schedulingOptions);
            }
            
            
            shiftList = _commonMainShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(shiftList,
																					schedulingOptions
																						.MainShiftOptimizeActivitySpecification);

			shiftList = _shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, shiftList, finderResult);
			shiftList = _disallowedShiftCategoriesShiftFilter.Filter(schedulingOptions.NotAllowedShiftCategories, shiftList,
																	 finderResult);
			shiftList = _activityRestrictionsShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction,
																finderResult);
			shiftList = _timeLimitsRestrictionShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction,
																 finderResult);
			shiftList = _workTimeLimitationShiftFilter.Filter(shiftList, effectiveRestriction, finderResult);
			shiftList = _notOverWritableActivitiesShiftFilter.Filter(dateOnly, person, shiftList, finderResult);
			
			
			return shiftList;
		}

		private IList<IShiftProjectionCache> runFiltersForRoleModel2(IList<IShiftProjectionCache> shiftList, ITeamBlockInfo teamBlockInfo, IWorkShiftFinderResult finderResult)
		{
			foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
			{
				foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					shiftList = _personalShiftsShiftFilter.Filter(dateOnly, person, shiftList, finderResult);
				}
			}
			
			return shiftList;
		}

		private IList<IShiftProjectionCache> runFilters(DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
								 ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult, IList<IShiftProjectionCache> shiftList,
								 IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameContractTime)
		{
			shiftList = _businessRulesShiftFilter.Filter(person, shiftList, dateOnly, finderResult);
			shiftList = _commonActivityFilter.Filter(shiftList, schedulingOptions, effectiveRestriction);
			shiftList = _personalShiftsShiftFilter.Filter(dateOnly, person, shiftList, finderResult);
			shiftList = runFiltersForRoleModel(dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person, matrixList,sameContractTime );
			return shiftList;
		}
	}
}