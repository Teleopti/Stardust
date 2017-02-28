﻿using System.Collections.Generic;
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
	public interface ITeamBlockRoleModelSelector
	{
		IShiftProjectionCache Select(IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, DateOnly dateTime, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction additionalEffectiveRestriction, IGroupPersonSkillAggregator groupPersonSkillAggregator);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlock _sameOpenHoursInTeamBlock;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ISameOpenHoursInTeamBlock sameOpenHoursInTeamBlock,
			IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlock = sameOpenHoursInTeamBlock;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
		}

		public IShiftProjectionCache Select(IScheduleDictionary schedules,
																	IEnumerable<ISkillDay> allSkillDays, 
																	IWorkShiftSelector workShiftSelector, 
																	ITeamBlockInfo teamBlockInfo, 
																	DateOnly datePointer, 
																	IPerson person, 
																	ISchedulingOptions schedulingOptions, 
																	IEffectiveRestriction additionalEffectiveRestriction,
																	IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(schedules, datePointer, person, teamBlockInfo, schedulingOptions);
			if (effectiveRestriction == null)
				return null;

			IShiftProjectionCache foundShiftProjectionCache = _firstShiftInTeamBlockFinder.FindFirst(teamBlockInfo, person, datePointer, schedules);
			if (foundShiftProjectionCache != null &&
			    !schedulingOptions.NotAllowedShiftCategories.Contains(foundShiftProjectionCache.TheMainShift.ShiftCategory))
				return foundShiftProjectionCache;
			
			effectiveRestriction = effectiveRestriction.Combine(additionalEffectiveRestriction);

			var roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, false, groupPersonSkillAggregator);
			if(roleModel == null && effectiveRestriction!= null && effectiveRestriction.IsRestriction)
				roleModel = filterAndSelect(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, true, groupPersonSkillAggregator);

			return roleModel;
		}

		private IShiftProjectionCache filterAndSelect(IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, bool useShiftsForRestrictions, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlock.Check(allSkillDays, teamBlockInfo, groupPersonSkillAggregator);
			var shifts = _workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, schedules, datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupMembers.First(), datePointer),
				isSameOpenHoursInBlock, useShiftsForRestrictions, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return null;

			return workShiftSelector.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), true, teamBlockInfo.TeamInfo.GroupMembers.First());
		}
	}
}