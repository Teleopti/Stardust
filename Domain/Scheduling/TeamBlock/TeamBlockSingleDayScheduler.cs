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
		bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
											   IList<IPerson> selectedPersons, DateOnly day,
											   IShiftProjectionCache roleModelShift, DateOnlyPeriod selectedPeriod, 
												ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
												IResourceCalculateDelayer resourceCalculateDelayer,
												ISchedulingResultStateHolder schedulingResultStateHolder);

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
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
										   IProposedRestrictionAggregator proposedRestrictionAggregator,
										   IWorkShiftFilterService workShiftFilterService,
										   IWorkShiftSelector workShiftSelector,
										   ITeamScheduling teamScheduling,
										   ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
											IActivityIntervalDataCreator activityIntervalDataCreator)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamScheduling = teamScheduling;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_activityIntervalDataCreator = activityIntervalDataCreator;
		}

		public bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
									  IList<IPerson> selectedPersons, DateOnly day,
									  IShiftProjectionCache roleModelShift, DateOnlyPeriod selectedPeriod, 
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			if (roleModelShift == null) return false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);
			
			var bestShiftProjectionCache = roleModelShift;
			var groupMembers = teamInfo.GroupMembers;
			var selectedTeamMembers = groupMembers.Intersect(selectedPersons).ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			if (isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo))
				return true;
			foreach (var person in selectedTeamMembers)
			{
				if (_cancelMe)
					return false;
				if (isTeamBlockScheduledForSelectedTeamMembers(new List<IPerson> { person }, day, teamBlockSingleDayInfo))
					continue;
				if (!_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions) && !_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(schedulingOptions))
				{
					var restriction = _proposedRestrictionAggregator.Aggregate(schedulingOptions, teamBlockInfo, day, person, roleModelShift);
					if (restriction == null) return false;
					var shifts = _workShiftFilterService.Filter(day, person, teamBlockSingleDayInfo, restriction, schedulingOptions,
					                                            new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupMembers.First(), day));
					if (shifts.IsNullOrEmpty()) continue;

					var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockInfo, day, schedulingResultStateHolder);
						if (dataForActivity.Any())
							activityInternalData.Add(activity, dataForActivity);

					bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																								 schedulingOptions
																									 .WorkShiftLengthHintOption,
																								 schedulingOptions
																									 .UseMinimumPersons,
																								 schedulingOptions
																									 .UseMaximumPersons, TimeZoneGuard.Instance.TimeZone);
					if (bestShiftProjectionCache == null) continue;
				}

				_teamScheduling.DayScheduled += OnDayScheduled;
				_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache, selectedPeriod, schedulePartModifyAndRollbackService, resourceCalculateDelayer);
				_teamScheduling.DayScheduled -= OnDayScheduled;
			}

			return isTeamBlockScheduledForSelectedTeamMembers(selectedTeamMembers, day, teamBlockSingleDayInfo);
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
