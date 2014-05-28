using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSingleDayScheduler
	{
		bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, 
												ISchedulingOptions schedulingOptions,
												DateOnly day,
												IShiftProjectionCache roleModelShift, 
												ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
												IResourceCalculateDelayer resourceCalculateDelayer,
												ISchedulingResultStateHolder schedulingResultStateHolder,
												IEffectiveRestriction shiftNudgeRestriction);

		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}

	public class TeamBlockSingleDayScheduler : ITeamBlockSingleDayScheduler
	{
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly IProposedRestrictionAggregator _proposedRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ITeamScheduling _teamScheduling;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IActivityIntervalDataCreator _activityIntervalDataCreator;
		private bool _cancelMe;
		private IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly IToggleManager _toggleManager;

		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
										   IProposedRestrictionAggregator proposedRestrictionAggregator,
										   IWorkShiftFilterService workShiftFilterService,
										   IWorkShiftSelector workShiftSelector,
										   ITeamScheduling teamScheduling,
										   ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
											IActivityIntervalDataCreator activityIntervalDataCreator, IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals, IToggleManager toggleManager)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamScheduling = teamScheduling;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
			_toggleManager = toggleManager;
		}

		public bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, 
										ISchedulingOptions schedulingOptions,
										DateOnly day,
										IShiftProjectionCache roleModelShift, 
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder,
										IEffectiveRestriction shiftNudgeRestriction)
		{
			_cancelMe = false;
			if (roleModelShift == null) return false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);
			
			var bestShiftProjectionCache = roleModelShift;
			var selectedTeamMembers = teamInfo.UnLockedMembers().ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			if (isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo))
				return true;
			foreach (var person in selectedTeamMembers)
			{
				if (_cancelMe)
					return false;

				if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> { person }, day, teamBlockSingleDayInfo))
					continue;

				var isSingleAgentTeamAndBlockWithSameShift = !schedulingOptions.UseTeam && schedulingOptions.UseBlock &&
				                                             schedulingOptions.BlockSameShift;
				if (!isSingleAgentTeamAndBlockWithSameShift)
				{
					var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person,
						roleModelShift);
					if (restriction == null) return false;
					restriction = restriction.Combine(shiftNudgeRestriction);
					var shifts = _workShiftFilterService.FilterForTeamMember(day, person, teamBlockSingleDayInfo, restriction,
						schedulingOptions,
						new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day));
					if (shifts.IsNullOrEmpty()) continue;

					var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockSingleDayInfo, day,
						schedulingResultStateHolder, false);
					var maxSeatFeatureOption = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats;
					IDictionary<DateTime, bool> maxSeatInfo = new Dictionary<DateTime, bool>();
					maxSeatFeatureOption = handleMaxSeatFeature(teamBlockInfo, schedulingOptions, day, schedulingResultStateHolder, maxSeatFeatureOption, ref maxSeatInfo);
						
					var parameters = new PeriodValueCalculationParameters(schedulingOptions
						.WorkShiftLengthHintOption, schedulingOptions
							.UseMinimumPersons,
						schedulingOptions
							.UseMaximumPersons, maxSeatFeatureOption);
					parameters.MaxSeatInfoPerInterval = maxSeatInfo;
					bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
						parameters, TimeZoneGuard.Instance.TimeZone);
					if (bestShiftProjectionCache == null) continue;
				}

				_teamScheduling.DayScheduled += OnDayScheduled;
				_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache,
					schedulePartModifyAndRollbackService, resourceCalculateDelayer);
				_teamScheduling.DayScheduled -= OnDayScheduled;
			}

			return isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo);
		}

		private MaxSeatsFeatureOptions handleMaxSeatFeature(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			DateOnly day, ISchedulingResultStateHolder schedulingResultStateHolder, MaxSeatsFeatureOptions maxSeatFeatureOption,
			ref IDictionary<DateTime, bool> maxSeatInfo)
		{
			if (_toggleManager.IsEnabled(Toggles.Scheduler_TeamBlockAdhereWithMaxSeatRule_23419))
			{
				if (schedulingOptions.UseMaxSeats)
				{
					maxSeatFeatureOption = MaxSeatsFeatureOptions.ConsiderMaxSeats;
					if (schedulingOptions.DoNotBreakMaxSeats)
						maxSeatFeatureOption = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
				}

				maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, day,
					schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone);
			}
			return maxSeatFeatureOption;
		}


		public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;
		}

		private bool isTeamBlockScheduledForSelectedTeamMembers(IList<IPerson> selectedTeamMembers, DateOnly day, ITeamBlockInfo teamBlockSingleDayInfo)
		{
			return _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockSingleDayInfo, day, selectedTeamMembers);
		}
	}
}
