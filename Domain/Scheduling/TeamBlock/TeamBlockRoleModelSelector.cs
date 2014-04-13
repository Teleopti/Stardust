using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockRoleModelSelector
	{
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly firstSelectedDayInBlock, IPerson person, ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
										  IWorkShiftFilterService workShiftFilterService,
										  ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
										  IWorkShiftSelector workShiftSelector,
											IDayIntervalDataCalculator dayIntervalDataCalculator,
											ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity,
											ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly firstSelectedDayInBlock, IPerson person, ISchedulingOptions schedulingOptions)
		{
			if (teamBlockInfo == null)
				return null;
			if (schedulingOptions == null)
				return null;

			var datePointer = firstSelectedDayInBlock;
			var restriction = _teamBlockRestrictionAggregator.Aggregate(datePointer, person, teamBlockInfo, schedulingOptions);
			if (restriction == null)
				return null;

			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, restriction,
																	schedulingOptions,
																	new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer),
																	isSameOpenHoursInBlock);
			if (shifts.IsNullOrEmpty())
				return null;


			//transform
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo,
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
					var dateDic = skillIntervalDataPerDateAndActivity[dateOnly];
					if (!dateDic.ContainsKey(activity))
						continue;

					dateOnlyDicForActivity.Add(dateOnly, dateDic[activity]);
				}

				IDictionary<DateTime, ISkillIntervalData> dataForActivity = _dayIntervalDataCalculator.Calculate(dateOnlyDicForActivity, datePointer);
				activityInternalData.Add(activity, dataForActivity);
			}

			var roleModel = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																		  schedulingOptions
																			  .WorkShiftLengthHintOption,
																		  schedulingOptions
																			  .UseMinimumPersons,
																		  schedulingOptions
																			  .UseMaximumPersons, TimeZoneGuard.Instance.TimeZone);
			return roleModel;
		}
	}
}