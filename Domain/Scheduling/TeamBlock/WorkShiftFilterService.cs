using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IWorkShiftFilterService
	{
		IList<ShiftProjectionCache> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, IScheduleDictionary schedules, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult, bool isSameOpenHoursInBlock, bool useShiftsForRestrictions, IEnumerable<ISkillDay> skillDays);
		IList<ShiftProjectionCache> FilterForTeamMember(IScheduleDictionary schedules, DateOnly dateOnly, IPerson person, ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult, bool useShiftsForRestrictions);
	}


	public class WorkShiftFilterService : IWorkShiftFilterService
	{
		private readonly ActivityRestrictionsShiftFilter _activityRestrictionsShiftFilter;
		private readonly BusinessRulesShiftFilter _businessRulesShiftFilter;
		private readonly CommonMainShiftFilter _commonMainShiftFilter;
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
		private readonly CommonActivityFilter _commonActivityFilter;
        private readonly IRuleSetAccordingToAccessabilityFilter _ruleSetAccordingToAccessabilityFilter;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IRuleSetPersonalSkillsActivityFilter _ruleSetPersonalSkillsActivityFilter;
		private readonly IDisallowedShiftProjectionCachesFilter _disallowedShiftProjectionCachesFilter;
		private readonly ActivityRequiresSkillProjectionFilter _activityRequiresSkillProjectionFilter;
		private readonly OpenHoursFilter _openHoursFilter;

		public WorkShiftFilterService(ActivityRestrictionsShiftFilter activityRestrictionsShiftFilter,
			BusinessRulesShiftFilter businessRulesShiftFilter,
			CommonMainShiftFilter commonMainShiftFilter,
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
			CommonActivityFilter commonActivityFilter, 
			IRuleSetAccordingToAccessabilityFilter ruleSetAccordingToAccessabilityFilter,
			IShiftProjectionCacheManager shiftProjectionCacheManager,
			IRuleSetPersonalSkillsActivityFilter ruleSetPersonalSkillsActivityFilter,
			IDisallowedShiftProjectionCachesFilter disallowedShiftProjectionCachesFilter,
			ActivityRequiresSkillProjectionFilter activityRequiresSkillProjectionFilter,
			OpenHoursFilter openHoursFilter)
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
			_disallowedShiftProjectionCachesFilter = disallowedShiftProjectionCachesFilter;
			_activityRequiresSkillProjectionFilter = activityRequiresSkillProjectionFilter;
			_openHoursFilter = openHoursFilter;
		}

		public IList<ShiftProjectionCache> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, IScheduleDictionary schedules, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
			IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult,
			bool isSameOpenHoursInBlock, bool useShiftsForRestrictions, IEnumerable<ISkillDay> skillDays)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.Where(x => x.Period(dateOnly) != null);
			var person = groupMembers.First();
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly).ToList();
			if (matrixList.Count == 0) return null;
			//var firstMember = teamBlockInfo.TeamInfo.GroupMembers.First();
			var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction, finderResult))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForRoleModel(groupPersonSkillAggregator, teamBlockInfo, schedulingOptions, useShiftsForRestrictions).ToList();
			filteredRuleSetList = _ruleSetPersonalSkillsActivityFilter.FilterForRoleModel(filteredRuleSetList,
				teamBlockInfo.TeamInfo, dateOnly).ToList();

			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, false, false);

			shiftList = runFiltersForRoleModel(schedules, dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList,
				person, matrixList, isSameOpenHoursInBlock);
			shiftList = runPersonalShiftFilterForEachMember(schedules, shiftList, teamBlockInfo, finderResult);
			if(schedulingOptions.UseBlock && schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.BetweenDayOff && !schedulingOptions.BlockSameShiftCategory)		
				shiftList = _businessRulesShiftFilter.Filter(schedules, person, shiftList, dateOnly, finderResult);			

			if (allowanceToUseBlackList(shiftList, schedulingOptions, effectiveRestriction))
			{
				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, true, false);
				shiftList = runFiltersForRoleModel(schedules, dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList,
						person, matrixList, isSameOpenHoursInBlock);
				shiftList = runPersonalShiftFilterForEachMember(schedules, shiftList, teamBlockInfo, finderResult);
				if (schedulingOptions.UseBlock && schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.BetweenDayOff && !schedulingOptions.BlockSameShiftCategory)
					shiftList = _businessRulesShiftFilter.Filter(schedules, person, shiftList, dateOnly, finderResult);		
			}

			if (schedulingOptions.UseBlock && schedulingOptions.BlockSameShift)
			{
				shiftList = _openHoursFilter.Filter(shiftList, skillDays, person, dateOnly);
			}

			if (shiftList == null)
				return null;
			return shiftList.Count == 0 ? null : shiftList;
		}

		public IList<ShiftProjectionCache> FilterForTeamMember(IScheduleDictionary schedules, DateOnly dateOnly, IPerson person,
			ITeamBlockInfo teamBlockInfo, IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions,
			IWorkShiftFinderResult finderResult, bool useShiftsForRestrictions)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
			var matrixList =
				teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, dateOnly.ToDateOnlyPeriod()).ToList();
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

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForTeamMember(person, dateOnly, schedulingOptions, useShiftsForRestrictions);
			filteredRuleSetList = _ruleSetPersonalSkillsActivityFilter.Filter(filteredRuleSetList, person.Period(dateOnly), dateOnly);

			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, false, false);

			shiftList = runFilters(schedules, dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person, matrixList,
				true);

			if (allowanceToUseBlackList(shiftList, schedulingOptions, effectiveRestriction))
			{
				shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, true, false);
				shiftList = runFilters(schedules, dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person,
					matrixList, true);
			}

			shiftList = _disallowedShiftProjectionCachesFilter.Filter(schedulingOptions.NotAllowedShiftProjectionCaches, shiftList, finderResult);

			if (shiftList == null)
				return null;
			return shiftList.Count == 0 ? null : shiftList;
		}

		private bool allowanceToUseBlackList(IList<ShiftProjectionCache> shiftList, SchedulingOptions schedulingOptions,
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

		private IList<ShiftProjectionCache> runFiltersForRoleModel(IScheduleDictionary schedules, DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
														SchedulingOptions schedulingOptions,
														IWorkShiftFinderResult finderResult,
														IList<ShiftProjectionCache> shiftList,
														IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameOpenHours)
		{
			shiftList = _shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, shiftList, finderResult);
			shiftList = _timeLimitsRestrictionShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction, finderResult);
            if (sameOpenHours)
            {
                shiftList = _contractTimeShiftFilter.Filter(dateOnly, matrixList, shiftList, schedulingOptions, finderResult);
	            var matrixForPerson = matrixList.FirstOrDefault(scheduleMatrixPro => scheduleMatrixPro.Person.Equals(person));
	            if (matrixForPerson != null)
	            {
					shiftList = _shiftLengthDecider.FilterList(shiftList, _minMaxCalculator, matrixForPerson, schedulingOptions);
				}
			}

            shiftList = _commonMainShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(shiftList, schedulingOptions.MainShiftOptimizeActivitySpecification);

			
			shiftList = _disallowedShiftCategoriesShiftFilter.Filter(schedulingOptions.NotAllowedShiftCategories, shiftList,
																	 finderResult);
			
			shiftList = _workTimeLimitationShiftFilter.Filter(shiftList, effectiveRestriction, finderResult);

			shiftList = _activityRequiresSkillProjectionFilter.Filter(person, shiftList, dateOnly, finderResult);
			shiftList = _activityRestrictionsShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction, finderResult);
			shiftList = _notOverWritableActivitiesShiftFilter.Filter(schedules, dateOnly, person, shiftList, finderResult);

			return shiftList;
		}

		private IList<ShiftProjectionCache> runPersonalShiftFilterForEachMember(IScheduleDictionary schedules, IList<ShiftProjectionCache> shiftList, ITeamBlockInfo teamBlockInfo, IWorkShiftFinderResult finderResult)
		{
			var blockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			foreach (var person in teamBlockInfo.TeamInfo.GroupMembers)
			{
				foreach (var dateOnly in blockDays)
				{
					shiftList = _personalShiftsShiftFilter.Filter(schedules, dateOnly, person, shiftList, finderResult);
				}
			}

			return shiftList;
		}

		private IList<ShiftProjectionCache> runFilters(IScheduleDictionary schedules, DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
								 SchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult, IList<ShiftProjectionCache> shiftList,
								 IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameContractTime)
		{		
			shiftList = runFiltersForRoleModel(schedules, dateOnly, effectiveRestriction, schedulingOptions, finderResult, shiftList, person, matrixList,sameContractTime );
			shiftList = _businessRulesShiftFilter.Filter(schedules, person, shiftList, dateOnly, finderResult);
			shiftList = _commonActivityFilter.Filter(shiftList, schedulingOptions, effectiveRestriction);
			shiftList = _personalShiftsShiftFilter.Filter(schedules, dateOnly, person, shiftList, finderResult);
			return shiftList;
		}
	}
}