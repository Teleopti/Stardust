using System;
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
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			IEffectiveRestriction shiftNudgeRestriction);

		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

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

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		
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
			if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> { person }, day, teamBlockSingleDayInfo))
				return resultList;

			var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);

			if (restriction == null)
				return resultList;

			var shifts = _workShiftFilterService.FilterForTeamMember(day, person, teamBlockSingleDayInfo, restriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day));

			if (shifts.IsNullOrEmpty())
				return resultList;

			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockSingleDayInfo, day,
				schedulingResultStateHolder, false);


			IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfo =
						_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day, schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone, true);
			

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
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			IEffectiveRestriction shiftNudgeRestriction)
		{
			var cancelMe = false;
			if (roleModelShift == null) return false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);

			var bestShiftProjectionCache = roleModelShift;
			var selectedTeamMembers = teamInfo.UnLockedMembers().ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			if (isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo))
				return true;

			EventHandler<SchedulingServiceBaseEventArgs> onDayScheduled = (sender, e) =>
			{
				EventHandler<SchedulingServiceBaseEventArgs> handler = DayScheduled;
				if (handler != null)
				{
					e.AppendCancelAction(()=>cancelMe=true);
					handler(this, e);
					if (e.Cancel) cancelMe = true;
				}
			};

			foreach (var person in selectedTeamMembers)
			{
				if (cancelMe) return false;

				if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> {person}, day, teamBlockSingleDayInfo))
					continue;

				var isSingleAgentTeamAndBlockWithSameShift = !schedulingOptions.UseTeam && schedulingOptions.UseBlock &&
				                                             schedulingOptions.BlockSameShift;
				if (!isSingleAgentTeamAndBlockWithSameShift)
				{
					var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);
					if (restriction == null) return false;
					if (shiftNudgeRestriction != null)
						restriction = restriction.Combine(shiftNudgeRestriction);

					var shifts = _workShiftFilterService.FilterForTeamMember(day, person, teamBlockSingleDayInfo, restriction,
						schedulingOptions,
						new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day));
					if (shifts.IsNullOrEmpty()) continue;

					var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockSingleDayInfo, day,
						schedulingResultStateHolder, false);

					IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfo =
						_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day, schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone, true);
			

					var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(),
						new DateOnlyPeriod(day, day));
					bool hasMaxSeatSkill = maxSeatSkills.Any();

					var parameters = new PeriodValueCalculationParameters(schedulingOptions
						.WorkShiftLengthHintOption, schedulingOptions.UseMinimumPersons,
						schedulingOptions.UseMaximumPersons, schedulingOptions.UserOptionMaxSeatsFeature, hasMaxSeatSkill, maxSeatInfo);
					bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
						parameters, TimeZoneGuard.Instance.TimeZone);
					if (bestShiftProjectionCache == null) continue;
				}

				_teamScheduling.DayScheduled += onDayScheduled;
				_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache,
					schedulePartModifyAndRollbackService, resourceCalculateDelayer, false);
				_teamScheduling.DayScheduled -= onDayScheduled;
			}

			return isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo);
		}

		private bool isTeamBlockScheduledForSelectedTeamMembers(IList<IPerson> selectedTeamMembers, DateOnly day,
			ITeamBlockInfo teamBlockSingleDayInfo)
		{
			return _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockSingleDayInfo, day,
				selectedTeamMembers);
		}
	}
}
