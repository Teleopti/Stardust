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
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly dateTime, IPerson person, ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private readonly IWorkShiftSelector _workShiftSelector;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
										  ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
										  IWorkShiftFilterService workShiftFilterService,
										  ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
										  IWorkShiftSelector workShiftSelector)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
		}

		public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, IPerson person, ISchedulingOptions schedulingOptions)
		{
			if (teamBlockInfo == null)
				return null;
			if (schedulingOptions == null)
				return null;
			var restriction = _teamBlockRestrictionAggregator.Aggregate(datePointer, person, teamBlockInfo, schedulingOptions);
			if (restriction == null)
				return null;
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, restriction,
																	schedulingOptions,
																	new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupMembers.First(), datePointer),
																	_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo));
			if (shifts.IsNullOrEmpty())
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