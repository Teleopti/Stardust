using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockSingleDayScheduler
	{
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ProposedRestrictionAggregator _proposedRestrictionAggregator;
		private readonly WorkShiftFilterService _workShiftFilterService;
		private readonly TeamScheduling _teamScheduling;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			ProposedRestrictionAggregator proposedRestrictionAggregator,
			WorkShiftFilterService workShiftFilterService,
			TeamScheduling teamScheduling,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_teamScheduling = teamScheduling;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public bool ScheduleSingleDay(IEnumerable<IPersonAssignment> orginalPersonAssignments, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, DateOnly day,
			ShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skills, IScheduleDictionary schedules, ResourceCalculationData resourceCalculationData,
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

				var isSingleAgentTeamAndBlockWithSameShift = !schedulingOptions.UseTeam && schedulingOptions.UseBlock && schedulingOptions.BlockSameShift ||
							schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SingleDay && schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type == GroupPageType.SingleAgent;
				if (!isSingleAgentTeamAndBlockWithSameShift || schedulingOptions.ShiftBagBackToLegal)
				{
					var restriction = _proposedRestrictionAggregator.Aggregate(schedules, schedulingOptions, teamBlockInfo, day, person, roleModelShift);
					if (restriction == null)
						return false;
					var allSkillDays = skills.ToSkillDayEnumerable();

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
				cancelMe = _teamScheduling.ExecutePerDayPerPerson(orginalPersonAssignments, person, day, teamBlockInfo, bestShiftProjectionCache,
					schedulePartModifyAndRollbackService, businessRules, schedulingOptions, schedules, resourceCalculationData, dayScheduled);
			}

			return isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo, schedulingOptions);
		}

		private bool isTeamBlockScheduledForSelectedTeamMembers(IList<IPerson> selectedTeamMembers, DateOnly day,
			ITeamBlockInfo teamBlockSingleDayInfo, SchedulingOptions schedulingOptions)
		{
			return _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockSingleDayInfo, day, selectedTeamMembers, schedulingOptions);
		}

		private ShiftProjectionCache filterAndSelect(IWorkShiftSelector workShiftSelector, SchedulingOptions schedulingOptions, DateOnly day,
			IPerson person, TeamBlockSingleDayInfo teamBlockSingleDayInfo,
			IEffectiveRestriction restriction, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules, bool useShiftsForRestrictions)
		{
			var shifts = _workShiftFilterService.FilterForTeamMember(schedules, day, person, teamBlockSingleDayInfo, restriction,
						schedulingOptions, useShiftsForRestrictions, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return null;

			return workShiftSelector.SelectShiftProjectionCache(_groupPersonSkillAggregator, day, shifts, allSkillDays, teamBlockSingleDayInfo, schedulingOptions, TimeZoneGuard.Instance.CurrentTimeZone(), false, person);
		}
	}
}
