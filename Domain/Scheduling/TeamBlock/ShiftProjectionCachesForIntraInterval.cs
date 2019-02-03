using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ShiftProjectionCachesForIntraInterval
	{
		private readonly TeamBlockRoleModelSelector _roleModelSelector;
		private readonly ActivityIntervalDataCreator _activityIntervalDataCreator;
		private readonly IWorkShiftSelectorForIntraInterval _workSelectorForIntraInterval;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ProposedRestrictionAggregator _proposedRestrictionAggregator;
		private readonly WorkShiftFilterService _workShiftFilterService;

		public ShiftProjectionCachesForIntraInterval(TeamBlockRoleModelSelector roleModelSelector,
			ActivityIntervalDataCreator activityIntervalDataCreator,
			IWorkShiftSelectorForIntraInterval workSelectorForIntraInterval,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			ProposedRestrictionAggregator proposedRestrictionAggregator,
			WorkShiftFilterService workShiftFilterService)
		{
			_roleModelSelector = roleModelSelector;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_workSelectorForIntraInterval = workSelectorForIntraInterval;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_proposedRestrictionAggregator = proposedRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
		}

		public IEnumerable<IWorkShiftCalculationResultHolder> Execute(
			ITeamBlockInfo teamBlockInfo,
			IPerson person,
			DateOnly datePointer,
			SchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return Enumerable.Empty<IWorkShiftCalculationResultHolder>();

			var roleModelShift = _roleModelSelector.Select(schedulingResultStateHolder.Schedules, schedulingResultStateHolder.SkillDays.ToSkillDayEnumerable(), _workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(), schedulingOptions, new EffectiveRestriction(), _groupPersonSkillAggregator);

			return roleModelShift == null ? 
				Enumerable.Empty<IWorkShiftCalculationResultHolder>() : 
				shiftProjectionCaches(teamBlockInfo, schedulingOptions, datePointer, roleModelShift, schedulingResultStateHolder, person);
		}

		private IEnumerable<IWorkShiftCalculationResultHolder> shiftProjectionCaches(
			ITeamBlockInfo teamBlockInfo,
			SchedulingOptions schedulingOptions,
			DateOnly day,
			ShiftProjectionCache roleModelShift,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person)
		{
			//TODO: should probably consider "IsClassic" here...
			var isSingleAgentTeamAndBlockWithSameShift = !schedulingOptions.UseTeam && schedulingOptions.UseBlock &&
														 schedulingOptions.BlockSameShift;
			if (isSingleAgentTeamAndBlockWithSameShift)
				return Enumerable.Empty<IWorkShiftCalculationResultHolder>();

			var teamInfo = teamBlockInfo.TeamInfo;
			var teamBlockSingleDayInfo = new TeamBlockSingleDayInfo(teamInfo, day);

			if(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockSingleDayInfo, day, new List<IPerson> { person }, schedulingOptions))
				return Enumerable.Empty<IWorkShiftCalculationResultHolder>();

			var restriction = _proposedRestrictionAggregator.Aggregate(schedulingResultStateHolder.Schedules, schedulingOptions, teamBlockInfo, day, person,
				roleModelShift);

			if (restriction == null)
				return Enumerable.Empty<IWorkShiftCalculationResultHolder>();

			var allSkillDays = schedulingResultStateHolder.SkillDays.ToSkillDayEnumerable();

			var shifts = _workShiftFilterService.FilterForTeamMember(schedulingResultStateHolder.Schedules, day, person, teamBlockSingleDayInfo, restriction,
				schedulingOptions, false, allSkillDays);

			if (shifts.IsNullOrEmpty())
				return Enumerable.Empty<IWorkShiftCalculationResultHolder>();

			var agentTimeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var activityInternalData = _activityIntervalDataCreator.CreateForAgent(_groupPersonSkillAggregator, teamBlockSingleDayInfo, day, allSkillDays, false, agentTimeZoneInfo);


			var parameters = new PeriodValueCalculationParameters(schedulingOptions.WorkShiftLengthHintOption, schedulingOptions.UseMinimumStaffing, schedulingOptions.UseMaximumStaffing);

			return _workSelectorForIntraInterval.SelectAllShiftProjectionCaches(shifts, activityInternalData,parameters, agentTimeZoneInfo);
		}
	}
}