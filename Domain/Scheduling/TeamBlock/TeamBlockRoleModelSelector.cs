using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockRoleModelSelector
	{
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly firstSelectedDayInBlock, IPerson person, ISchedulingOptions schedulingOptions);
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly dateTime, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IActivityIntervalDataCreator _activityIntervalDataCreator;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
										  IWorkShiftFilterService workShiftFilterService,
										  ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
										  IWorkShiftSelector workShiftSelector,
											ISchedulingResultStateHolder schedulingResultStateHolder,
											IActivityIntervalDataCreator activityIntervalDataCreator)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_activityIntervalDataCreator = activityIntervalDataCreator;
		}

        public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, IPerson person, ISchedulingOptions schedulingOptions)
		{
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(datePointer, person, teamBlockInfo, schedulingOptions);
			if (effectiveRestriction == null)
				return null;

			return Select(teamBlockInfo, datePointer, person, schedulingOptions, effectiveRestriction);
		}

        public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, IPerson person,
			ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction)
		{
			if (teamBlockInfo == null)
				return null;
			if (schedulingOptions == null)
				return null;

			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupMembers.First(), datePointer),
				isSameOpenHoursInBlock);
			if (shifts.IsNullOrEmpty())
				return null;

			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockInfo, datePointer,
				_schedulingResultStateHolder, true);

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