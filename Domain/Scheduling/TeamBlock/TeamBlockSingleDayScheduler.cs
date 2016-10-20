﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSingleDayScheduler
	{
		bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, DateOnly day,
			IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays,
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
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ITeamScheduling _teamScheduling;
		private readonly IActivityIntervalDataCreator _activityIntervalDataCreator;
		private readonly IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private readonly IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		
		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			IProposedRestrictionAggregator proposedRestrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			IWorkShiftSelector workShiftSelector, 
			ITeamScheduling teamScheduling,
			IActivityIntervalDataCreator activityIntervalDataCreator,
			IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals,
			IMaxSeatSkillAggregator maxSeatSkillAggregator)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamScheduling = teamScheduling;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
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

			var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);

			if (restriction == null)
				return resultList;

			var shifts = _workShiftFilterService.FilterForTeamMember(day, person, teamBlockSingleDayInfo, restriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day), false);

			if (shifts.IsNullOrEmpty())
				return resultList;

			var allSkillDays = schedulingResultStateHolder.AllSkillDays();
			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockSingleDayInfo, day, allSkillDays, false);
			var maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day, allSkillDays, TimeZoneGuard.Instance.TimeZone, true);
			

			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(),
				new DateOnlyPeriod(day, day));
			bool hasMaxSeatSkill = maxSeatSkills.Any();

			var parameters = new PeriodValueCalculationParameters(schedulingOptions
				.WorkShiftLengthHintOption, schedulingOptions.UseMinimumPersons,
				schedulingOptions.UseMaximumPersons, schedulingOptions.UserOptionMaxSeatsFeature, hasMaxSeatSkill, maxSeatInfo);

			resultList = _workShiftSelector.SelectAllShiftProjectionCaches(shifts, activityInternalData,
				parameters, TimeZoneGuard.Instance.TimeZone);

			return resultList;
		}

		public bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, DateOnly day,
			IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays,
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
					var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);
					if (restriction == null)
						return false;

					if (shiftNudgeRestriction != null)
						restriction = restriction.Combine(shiftNudgeRestriction);

					bestShiftProjectionCache = filterAndSelect(teamBlockInfo, schedulingOptions, day, person, teamBlockSingleDayInfo,
						restriction, allSkillDays, false);

					if (bestShiftProjectionCache == null && restriction.IsRestriction)
						bestShiftProjectionCache = filterAndSelect(teamBlockInfo, schedulingOptions, day, person, teamBlockSingleDayInfo,
						restriction, allSkillDays, true);

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

		private IShiftProjectionCache filterAndSelect(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, DateOnly day,
			IPerson person, TeamBlockSingleDayInfo teamBlockSingleDayInfo,
			IEffectiveRestriction restriction, IEnumerable<ISkillDay> allSkillDays, bool useShiftsForRestrictions)
		{
			var shifts = _workShiftFilterService.FilterForTeamMember(day, person, teamBlockSingleDayInfo, restriction,
						schedulingOptions,
						new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day), 
						useShiftsForRestrictions);

			if (shifts.IsNullOrEmpty())
				return null;

		
			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockSingleDayInfo, day, allSkillDays, false);
			var maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day, allSkillDays, TimeZoneGuard.Instance.TimeZone, true);

			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(),
				new DateOnlyPeriod(day, day));
			bool hasMaxSeatSkill = maxSeatSkills.Any();

			var parameters = new PeriodValueCalculationParameters(schedulingOptions
				.WorkShiftLengthHintOption, schedulingOptions.UseMinimumPersons,
				schedulingOptions.UseMaximumPersons, schedulingOptions.UserOptionMaxSeatsFeature, hasMaxSeatSkill, maxSeatInfo);

			return _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData, parameters, TimeZoneGuard.Instance.TimeZone, schedulingOptions);
		}
	}
}
