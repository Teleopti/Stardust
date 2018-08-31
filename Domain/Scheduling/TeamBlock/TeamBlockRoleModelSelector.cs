using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_XXL_76496)]
	public class TeamBlockRoleModelSelectorClearingProjections : TeamBlockRoleModelSelector
	{
		private readonly WorkShiftFilterService _workShiftFilterService;

		public TeamBlockRoleModelSelectorClearingProjections(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, WorkShiftFilterService workShiftFilterService, SameOpenHoursInTeamBlock sameOpenHoursInTeamBlock, FirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder) : base(teamBlockRestrictionAggregator, workShiftFilterService, sameOpenHoursInTeamBlock, firstShiftInTeamBlockFinder)
		{
			_workShiftFilterService = workShiftFilterService;
		}

		protected override ShiftProjectionCache filterAndSelect(IScheduleDictionary schedules,
			IEnumerable<ISkillDay> allSkillDays, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo,
			DateOnly datePointer, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction,
			bool useShiftsForRestrictions, IGroupPersonSkillAggregator groupPersonSkillAggregator,
			bool isSameOpenHoursInBlock)
		{
			var shifts = _workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, schedules, datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions, isSameOpenHoursInBlock, useShiftsForRestrictions, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return null;

			var res = workShiftSelector.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), true, teamBlockInfo.TeamInfo.GroupMembers.First());
			shifts.ClearMainShiftProjectionCaches(); //just a hack for perf/mem reasons to release resources... can this be made in a better way?
			return res;
		}
	}

	public class TeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly WorkShiftFilterService _workShiftFilterService;
		private readonly SameOpenHoursInTeamBlock _sameOpenHoursInTeamBlock;
		private readonly FirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			WorkShiftFilterService workShiftFilterService,
			SameOpenHoursInTeamBlock sameOpenHoursInTeamBlock,
			FirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlock = sameOpenHoursInTeamBlock;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
		}

		public ShiftProjectionCache Select(IScheduleDictionary schedules,
																	IEnumerable<ISkillDay> allSkillDays, 
																	IWorkShiftSelector workShiftSelector, 
																	ITeamBlockInfo teamBlockInfo, 
																	DateOnly datePointer, 
																	IPerson person, 
																	SchedulingOptions schedulingOptions, 
																	IEffectiveRestriction additionalEffectiveRestriction,
																	IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(schedules, datePointer, person, teamBlockInfo, schedulingOptions);
			if (effectiveRestriction == null)
				return null;

			ShiftProjectionCache foundShiftProjectionCache = _firstShiftInTeamBlockFinder.FindFirst(teamBlockInfo, person, datePointer, schedules);
			if (foundShiftProjectionCache != null &&
			    !schedulingOptions.NotAllowedShiftCategories.Contains(foundShiftProjectionCache.TheWorkShift.ShiftCategory))
				return foundShiftProjectionCache;
			
			effectiveRestriction = effectiveRestriction.Combine(additionalEffectiveRestriction);
			var adjustedStartTimeRestriction = new EffectiveRestriction();
			effectiveRestriction = effectiveRestriction?.Combine(adjustedStartTimeRestriction);
			if (effectiveRestriction == null) return null;

			//TODO: This check could probably be moved "higher up" for perf reasons/fewer calls
			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlock.Check(allSkillDays, teamBlockInfo, groupPersonSkillAggregator);
			var roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, false, groupPersonSkillAggregator, isSameOpenHoursInBlock);
			if(roleModel == null && effectiveRestriction!= null && effectiveRestriction.IsRestriction)
				roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, true, groupPersonSkillAggregator, isSameOpenHoursInBlock);

			return roleModel;
		}

		protected virtual ShiftProjectionCache filterAndSelect(IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, DateOnly datePointer, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, bool useShiftsForRestrictions, IGroupPersonSkillAggregator groupPersonSkillAggregator, bool isSameOpenHoursInBlock)
		{
			var shifts = _workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, schedules, datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions, isSameOpenHoursInBlock, useShiftsForRestrictions, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return null;

			return workShiftSelector.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), true, teamBlockInfo.TeamInfo.GroupMembers.First());
		}
	}
}