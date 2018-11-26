using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class WorkShiftFilterService
	{
		private readonly ActivityRestrictionsShiftFilter _activityRestrictionsShiftFilter;
		private readonly BusinessRulesShiftFilter _businessRulesShiftFilter;
		private readonly CommonMainShiftFilter _commonMainShiftFilter;
		private readonly ContractTimeShiftFilter _contractTimeShiftFilter;
		private readonly DisallowedShiftCategoriesShiftFilter _disallowedShiftCategoriesShiftFilter;
		private readonly EffectiveRestrictionShiftFilter _effectiveRestrictionShiftFilter;
		private readonly IMainShiftOptimizeActivitiesSpecificationShiftFilter _mainShiftOptimizeActivitiesSpecificationShiftFilter;
		private readonly INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;
		private readonly PersonalShiftsShiftFilter _personalShiftsShiftFilter;
		private readonly ShiftCategoryRestrictionShiftFilter _shiftCategoryRestrictionShiftFilter;
		private readonly TimeLimitsRestrictionShiftFilter _timeLimitsRestrictionShiftFilter;
		private readonly WorkTimeLimitationShiftFilter _workTimeLimitationShiftFilter;
		private readonly IShiftLengthDecider _shiftLengthDecider;
		private readonly IWorkShiftMinMaxCalculator _minMaxCalculator;
		private readonly CommonActivityFilter _commonActivityFilter;
        private readonly RuleSetAccordingToAccessabilityFilter _ruleSetAccordingToAccessabilityFilter;
		private readonly ShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IRuleSetPersonalSkillsActivityFilter _ruleSetPersonalSkillsActivityFilter;
		private readonly DisallowedShiftProjectionCachesFilter _disallowedShiftProjectionCachesFilter;
		private readonly ActivityRequiresSkillProjectionFilter _activityRequiresSkillProjectionFilter;
		private readonly OpenHoursFilter _openHoursFilter;
		private readonly IMaxWorkTimeFullWeekSkillExtractor _maxWorkTimeFullWeekSkillExtractor;

		public WorkShiftFilterService(ActivityRestrictionsShiftFilter activityRestrictionsShiftFilter,
			BusinessRulesShiftFilter businessRulesShiftFilter,
			CommonMainShiftFilter commonMainShiftFilter,
			ContractTimeShiftFilter contractTimeShiftFilter,
			DisallowedShiftCategoriesShiftFilter disallowedShiftCategoriesShiftFilter,
			EffectiveRestrictionShiftFilter effectiveRestrictionShiftFilter,
			IMainShiftOptimizeActivitiesSpecificationShiftFilter mainShiftOptimizeActivitiesSpecificationShiftFilter,
			INotOverWritableActivitiesShiftFilter notOverWritableActivitiesShiftFilter,
			PersonalShiftsShiftFilter personalShiftsShiftFilter,
			ShiftCategoryRestrictionShiftFilter shiftCategoryRestrictionShiftFilter,
			TimeLimitsRestrictionShiftFilter timeLimitsRestrictionShiftFilter,
			WorkTimeLimitationShiftFilter workTimeLimitationShiftFilter,
			IShiftLengthDecider shiftLengthDecider,
			IWorkShiftMinMaxCalculator minMaxCalculator,
			CommonActivityFilter commonActivityFilter, 
			RuleSetAccordingToAccessabilityFilter ruleSetAccordingToAccessabilityFilter,
			ShiftProjectionCacheManager shiftProjectionCacheManager,
			IRuleSetPersonalSkillsActivityFilter ruleSetPersonalSkillsActivityFilter,
			DisallowedShiftProjectionCachesFilter disallowedShiftProjectionCachesFilter,
			ActivityRequiresSkillProjectionFilter activityRequiresSkillProjectionFilter,
			OpenHoursFilter openHoursFilter,
			IMaxWorkTimeFullWeekSkillExtractor maxWorkTimeFullWeekSkillExtractor)
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
			_maxWorkTimeFullWeekSkillExtractor = maxWorkTimeFullWeekSkillExtractor;
		}

		public IList<ShiftProjectionCache> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, IScheduleDictionary schedules, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
			IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions, bool isSameOpenHoursInBlock, bool useShiftsForRestrictions, IEnumerable<ISkillDay> skillDays)
		{
			if (effectiveRestriction == null)
				return null;
			if (teamBlockInfo == null)
				return null;
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.Where(x => x.Period(dateOnly) != null);
			var person = groupMembers.First();
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly).ToList();
			if (matrixList.Count == 0) return null;
			var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
			if (!currentSchedulePeriod.IsValid)
				return null;
			if (schedulingOptions == null)
				return null;
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForRoleModel(groupPersonSkillAggregator, teamBlockInfo, schedulingOptions, useShiftsForRestrictions).ToList();
			filteredRuleSetList = _ruleSetPersonalSkillsActivityFilter.FilterForRoleModel(filteredRuleSetList,
				teamBlockInfo.TeamInfo, dateOnly).ToList();

			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, false);

			shiftList = runFiltersForRoleModel(schedules, dateOnly, effectiveRestriction, schedulingOptions, shiftList, person, matrixList, isSameOpenHoursInBlock, teamBlockInfo, skillDays);
			shiftList = runPersonalShiftFilterForEachMember(schedules, shiftList, teamBlockInfo);
			if(schedulingOptions.IsClassic() ||schedulingOptions.UseBlock && schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.BetweenDayOff && !schedulingOptions.BlockSameShiftCategory)		
				shiftList = _businessRulesShiftFilter.Filter(schedules, person, shiftList, dateOnly);

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
			bool useShiftsForRestrictions, IEnumerable<ISkillDay> skillDays)
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
			if (!_effectiveRestrictionShiftFilter.Filter(schedulingOptions, effectiveRestriction))
				return null;
			if (schedulingOptions.ShiftCategory != null)
				effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;

			var filteredRuleSetList = _ruleSetAccordingToAccessabilityFilter.FilterForTeamMember(person, dateOnly, schedulingOptions, useShiftsForRestrictions);
			filteredRuleSetList = _ruleSetPersonalSkillsActivityFilter.Filter(filteredRuleSetList, person.Period(dateOnly), dateOnly);

			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsPeriod, filteredRuleSetList, false);

			shiftList = runFilters(schedules, dateOnly, effectiveRestriction, schedulingOptions, shiftList, person, matrixList, true, teamBlockInfo, skillDays);

			shiftList = _disallowedShiftProjectionCachesFilter.Filter(schedulingOptions.NotAllowedShiftProjectionCaches, shiftList);

			if (schedulingOptions.UseTeam)
			{
				shiftList = _openHoursFilter.Filter(shiftList, skillDays, person, dateOnly);
			}

			if (shiftList == null)
				return null;
			return shiftList.Count == 0 ? null : shiftList;
		}

		private IList<ShiftProjectionCache> runFiltersForRoleModel(IScheduleDictionary schedules, DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
														SchedulingOptions schedulingOptions,
														IList<ShiftProjectionCache> shiftList,
														IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameOpenHours,
														ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays)
		{
			shiftList = _shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, shiftList);
			shiftList = _timeLimitsRestrictionShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction);
			shiftList = _mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(shiftList, schedulingOptions.MainShiftOptimizeActivitySpecification);
			if (sameOpenHours)
            {
                shiftList = _contractTimeShiftFilter.Filter(dateOnly, matrixList, shiftList, schedulingOptions);
	            var matrixForPerson = matrixList.FirstOrDefault(scheduleMatrixPro => scheduleMatrixPro.Person.Equals(person));
	            if (matrixForPerson != null)
	            {
					var maxWorkTimeDictionary = _maxWorkTimeFullWeekSkillExtractor.Extract(matrixForPerson, teamBlockInfo, skillDays);
					shiftList = _shiftLengthDecider.FilterList(shiftList, _minMaxCalculator, matrixForPerson, schedulingOptions, maxWorkTimeDictionary);
				}
			}

            shiftList = _commonMainShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _disallowedShiftCategoriesShiftFilter.Filter(schedulingOptions.NotAllowedShiftCategories, shiftList);
			shiftList = _workTimeLimitationShiftFilter.Filter(shiftList, effectiveRestriction);
			shiftList = _activityRequiresSkillProjectionFilter.Filter(person, shiftList, dateOnly);
			shiftList = _activityRestrictionsShiftFilter.Filter(dateOnly, person, shiftList, effectiveRestriction);
			shiftList = _notOverWritableActivitiesShiftFilter.Filter(schedules, dateOnly, person, shiftList);
			
			return shiftList;
		}

		private IList<ShiftProjectionCache> runPersonalShiftFilterForEachMember(IScheduleDictionary schedules, IList<ShiftProjectionCache> shiftList, ITeamBlockInfo teamBlockInfo)
		{
			var blockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			foreach (var person in teamBlockInfo.TeamInfo.GroupMembers)
			{
				foreach (var dateOnly in blockDays)
				{
					shiftList = _personalShiftsShiftFilter.Filter(schedules, dateOnly, person, shiftList);
				}
			}

			return shiftList;
		}

		private IList<ShiftProjectionCache> runFilters(IScheduleDictionary schedules, DateOnly dateOnly, IEffectiveRestriction effectiveRestriction,
									SchedulingOptions schedulingOptions, IList<ShiftProjectionCache> shiftList,
									IPerson person, IList<IScheduleMatrixPro> matrixList, bool sameContractTime,
									ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays)
		{		
			shiftList = runFiltersForRoleModel(schedules, dateOnly, effectiveRestriction, schedulingOptions, shiftList, person, matrixList,sameContractTime,teamBlockInfo, skillDays );
			shiftList = _businessRulesShiftFilter.Filter(schedules, person, shiftList, dateOnly);
			shiftList = _commonActivityFilter.Filter(shiftList, schedulingOptions, effectiveRestriction);
			shiftList = _personalShiftsShiftFilter.Filter(schedules, dateOnly, person, shiftList);
			return shiftList;
		}
	}
}