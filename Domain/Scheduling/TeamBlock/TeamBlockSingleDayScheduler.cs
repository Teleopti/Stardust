﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSingleDayScheduler
	{
		bool ScheduleSingleDay(IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, DateOnly day,
			IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules,
			IEffectiveRestriction shiftNudgeRestriction, INewBusinessRuleCollection businessRules, Func<SchedulingServiceBaseEventArgs, bool> dayScheduled);

		IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCaches(
			ITeamBlockInfo teamBlockInfo,
			ISchedulingOptions schedulingOptions,
			DateOnly day,
			IShiftProjectionCache roleModelShift,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person);
	}

	public class TeamBlockSingleDayScheduler : ITeamBlockSingleDayScheduler
	{
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly IProposedRestrictionAggregator _proposedRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ITeamScheduling _teamScheduling;
		private readonly IActivityIntervalDataCreator _activityIntervalDataCreator;
		private readonly IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private readonly IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private readonly IWorkShiftSelectorForIntraInterval _workSelectorForIntraInterval;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			IProposedRestrictionAggregator proposedRestrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ITeamScheduling teamScheduling,
			IActivityIntervalDataCreator activityIntervalDataCreator,
			IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals,
			IMaxSeatSkillAggregator maxSeatSkillAggregator,
			IWorkShiftSelectorForIntraInterval workSelectorForIntraInterval,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_teamScheduling = teamScheduling;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
			_workSelectorForIntraInterval = workSelectorForIntraInterval;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		// TODO Move to separate class
		public IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCaches(
			ITeamBlockInfo teamBlockInfo,
			ISchedulingOptions schedulingOptions, 
			DateOnly day,
			IShiftProjectionCache roleModelShift,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person)
		{
			IList<IWorkShiftCalculationResultHolder> resultList = new List<IWorkShiftCalculationResultHolder>();
			var isSingleAgentTeamAndBlockWithSameShift = !schedulingOptions.UseTeam && schedulingOptions.UseBlock &&
															 schedulingOptions.BlockSameShift;
			if (isSingleAgentTeamAndBlockWithSameShift)
				return resultList;

			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);
			if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> { person }, day, teamBlockSingleDayInfo, schedulingOptions))
				return resultList;

			var restriction = _proposedRestrictionAggregator.Aggregate(schedulingResultStateHolder.Schedules, schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);

			if (restriction == null)
				return resultList;

			var shifts = _workShiftFilterService.FilterForTeamMember(schedulingResultStateHolder.Schedules, day, person, teamBlockSingleDayInfo, restriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day), false);

			if (shifts.IsNullOrEmpty())
				return resultList;

			var allSkillDays = schedulingResultStateHolder.AllSkillDays();
			var activityInternalData = _activityIntervalDataCreator.CreateFor(_groupPersonSkillAggregator, teamBlockSingleDayInfo, day, allSkillDays, false);
			var maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day, allSkillDays, TimeZoneGuard.Instance.CurrentTimeZone(), true);
			

			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(),
				new DateOnlyPeriod(day, day));
			bool hasMaxSeatSkill = maxSeatSkills.Any();

			var parameters = new PeriodValueCalculationParameters(schedulingOptions
				.WorkShiftLengthHintOption, schedulingOptions.UseMinimumPersons,
				schedulingOptions.UseMaximumPersons, schedulingOptions.UserOptionMaxSeatsFeature, hasMaxSeatSkill, maxSeatInfo);

			resultList = _workSelectorForIntraInterval.SelectAllShiftProjectionCaches(shifts, activityInternalData,
				parameters, TimeZoneGuard.Instance.CurrentTimeZone());

			return resultList;
		}

		public bool ScheduleSingleDay(IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, DateOnly day,
			IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules,
			IEffectiveRestriction shiftNudgeRestriction, INewBusinessRuleCollection businessRules, Func<SchedulingServiceBaseEventArgs, bool> dayScheduled)
		{
			var cancelMe = false;
			if (roleModelShift == null) return false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);

			var bestShiftProjectionCache = roleModelShift;
			var selectedTeamMembers = teamInfo.UnLockedMembers(day).ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			if (isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo, schedulingOptions))
				return true;

			foreach (var person in selectedTeamMembers)
			{
				if (cancelMe)
					return false;

				if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> {person}, day, teamBlockSingleDayInfo, schedulingOptions))
					continue;

				var isSingleAgentTeamAndBlockWithSameShift = (!schedulingOptions.UseTeam && schedulingOptions.UseBlock &&
															  schedulingOptions.BlockSameShift) ||
															 (schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SingleDay &&
															  schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type == GroupPageType.SingleAgent);
				if (!isSingleAgentTeamAndBlockWithSameShift || schedulingOptions.ShiftBagBackToLegal)
				{
					var restriction = _proposedRestrictionAggregator.Aggregate(schedules, schedulingOptions, teamBlockInfo, day, person, roleModelShift);
					if (restriction == null)
						return false;

					if (shiftNudgeRestriction != null)
						restriction = restriction.Combine(shiftNudgeRestriction);

					bestShiftProjectionCache = filterAndSelect(workShiftSelector, schedulingOptions, day, person, teamBlockSingleDayInfo,
						restriction, allSkillDays, schedules, false);

					if (bestShiftProjectionCache == null && restriction.IsRestriction)
						bestShiftProjectionCache = filterAndSelect(workShiftSelector, schedulingOptions, day, person, teamBlockSingleDayInfo,
						restriction, allSkillDays, schedules, true);

					if (bestShiftProjectionCache == null)
						continue;
				}
				cancelMe = _teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache,
					schedulePartModifyAndRollbackService, resourceCalculateDelayer, false, businessRules, schedulingOptions, dayScheduled);
			}

			return isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo, schedulingOptions);
		}

		private bool isTeamBlockScheduledForSelectedTeamMembers(IList<IPerson> selectedTeamMembers, DateOnly day,
			ITeamBlockInfo teamBlockSingleDayInfo, ISchedulingOptions schedulingOptions)
		{
			return _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockSingleDayInfo, day, selectedTeamMembers, schedulingOptions);
		}

		private IShiftProjectionCache filterAndSelect(IWorkShiftSelector workShiftSelector, ISchedulingOptions schedulingOptions, DateOnly day,
			IPerson person, TeamBlockSingleDayInfo teamBlockSingleDayInfo,
			IEffectiveRestriction restriction, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules, bool useShiftsForRestrictions)
		{
			var shifts = _workShiftFilterService.FilterForTeamMember(schedules, day, person, teamBlockSingleDayInfo, restriction,
						schedulingOptions,
						new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day), 
						useShiftsForRestrictions);

			if (shifts.IsNullOrEmpty())
				return null;

			return workShiftSelector.SelectShiftProjectionCache(_groupPersonSkillAggregator, day, shifts, allSkillDays, teamBlockSingleDayInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), false, person);
		}
	}
}
