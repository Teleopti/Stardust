using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockRoleModelSelector
	{
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly dateTime, ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly IRestrictionAggregator _restrictionAggregator;
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private readonly IWorkShiftSelector _workShiftSelector;

		public TeamBlockRoleModelSelector(IRestrictionAggregator restrictionAggregator,
										  ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
										  IWorkShiftFilterService workShiftFilterService,
										  ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
										  IWorkShiftSelector workShiftSelector)
		{
			_restrictionAggregator = restrictionAggregator;
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
		}

		public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions)
		{
			if (teamBlockInfo == null)
				return null;
			if (schedulingOptions == null)
				return null;
			var restriction = _restrictionAggregator.Aggregate(teamBlockInfo, schedulingOptions);
			if (restriction == null)
				return null;
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, restriction,
																	schedulingOptions,
																	new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer),
																	_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo));
			if (shifts == null || shifts.Count <= 0)
				return null;
			var activityInternalData = _skillDayPeriodIntervalDataGenerator.GeneratePerDay(teamBlockInfo);
			var roleModel = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																		  schedulingOptions
																			  .WorkShiftLengthHintOption,
																		  schedulingOptions
																			  .UseMinimumPersons,
																		  schedulingOptions
																			  .UseMaximumPersons);
			return roleModel;
		}
	}
}