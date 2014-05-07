using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSingleDayScheduler
	{
		bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
											   IList<IPerson> selectedPersons, DateOnly day,
											   IShiftProjectionCache roleModelShift, DateOnlyPeriod selectedPeriod);

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
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private bool _cancelMe;
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamBlockSingleDayScheduler(ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
										   IProposedRestrictionAggregator proposedRestrictionAggregator,
										   IWorkShiftFilterService workShiftFilterService,
										   IWorkShiftSelector workShiftSelector,
										   ITeamScheduling teamScheduling,
										   ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
											IDayIntervalDataCalculator dayIntervalDataCalculator,
											ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity,
											ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamScheduling = teamScheduling;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public bool ScheduleSingleDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
									  IList<IPerson> selectedPersons, DateOnly day,
									  IShiftProjectionCache roleModelShift, DateOnlyPeriod selectedPeriod)
		{
			if (roleModelShift == null) return false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);
			
			var bestShiftProjectionCache = roleModelShift;
			var groupMembers = teamInfo.GroupPerson.GroupMembers;
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
					                                            new WorkShiftFinderResult(teamBlockSingleDayInfo.TeamInfo.GroupPerson, day));
					if (shifts.IsNullOrEmpty()) continue;

					//transform
					var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockSingleDayInfo,
																										   _schedulingResultStateHolder);
					
					var activities = new HashSet<IActivity>();
					foreach (var dicPerActivity in skillIntervalDataPerDateAndActivity.Values)
					{
						foreach (var activity in dicPerActivity.Keys)
						{
							activities.Add(activity);
						}
					}

					var activityInternalData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
					foreach (var activity in activities)
					{
						var dateOnlyDicForActivity = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
						foreach (var dateOnly in skillIntervalDataPerDateAndActivity.Keys)
						{
							if(dateOnly == day || dateOnly == day.AddDays(1))
							{
								var dateDic = skillIntervalDataPerDateAndActivity[dateOnly];
								if (!dateDic.ContainsKey(activity))
									continue;

								dateOnlyDicForActivity.Add(dateOnly, dateDic[activity]);
							}
						}

						IDictionary<DateTime, ISkillIntervalData> dataForActivity = _dayIntervalDataCalculator.Calculate(dateOnlyDicForActivity, day);
						activityInternalData.Add(activity, dataForActivity);
					}

					var activitiesToFilterFor = new List<IActivity>();
					foreach (var activity in activityInternalData.Keys)
					{
						activitiesToFilterFor.Add(activity);
					}

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
				_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache, selectedPeriod);
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
