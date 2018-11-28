using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly WorkShiftFilterService _workShiftFilterService;
		private readonly SameOpenHoursInTeamBlock _sameOpenHoursInTeamBlock;
		private readonly FirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private readonly IOpenHoursSkillExtractor _openHoursSkillExtractor;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			WorkShiftFilterService workShiftFilterService,
			SameOpenHoursInTeamBlock sameOpenHoursInTeamBlock,
			FirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder,
			IOpenHoursSkillExtractor openHoursSkillExtractor
			)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlock = sameOpenHoursInTeamBlock;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
			_openHoursSkillExtractor = openHoursSkillExtractor;
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
			var openHoursResult = _openHoursSkillExtractor.Extract(teamBlockInfo, allSkillDays, datePointer.ToDateOnlyPeriod());	
			if (openHoursResult != null && openHoursResult.OpenHoursDictionary.TryGetValue(datePointer, out var startEndRestriction))
			{
				effectiveRestriction = effectiveRestriction?.Combine(startEndRestriction);	
			}
			
			if (effectiveRestriction == null) return null;

			//TODO: This check could probably be moved "higher up" for perf reasons/fewer calls
			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlock.Check(allSkillDays, teamBlockInfo, groupPersonSkillAggregator);
			var roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, false, groupPersonSkillAggregator, isSameOpenHoursInBlock);
			if(roleModel == null && effectiveRestriction!= null && effectiveRestriction.IsRestriction)
				roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, true, groupPersonSkillAggregator, isSameOpenHoursInBlock);

			return roleModel;
		}

		private ShiftProjectionCache filterAndSelect(IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, DateOnly datePointer, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, bool useShiftsForRestrictions, IGroupPersonSkillAggregator groupPersonSkillAggregator, bool isSameOpenHoursInBlock)
		{
			var shifts = _workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, schedules, datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions, isSameOpenHoursInBlock, useShiftsForRestrictions, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return null;

			return workShiftSelector.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), true, teamBlockInfo.TeamInfo.GroupMembers.First());
		}
	}
}