using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSteadyStateValidator
	{
		bool IsTeamBlockInSteadyState(ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions);
	}

	public class TeamBlockSteadyStateValidator : ITeamBlockSteadyStateValidator
	{
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ISameStartTimeBlockSpecification _sameStartTimeBlockSpecification;
		private readonly ISameStartTimeTeamSpecification _sameStartTimeTeamSpecification;
		private readonly ISameEndTimeTeamSpecification _sameEndTimeTeamSpecification;
		private readonly ISameShiftCategoryBlockSpecification _sameShiftCategoryBlockSpecification;
		private readonly ISameShiftCategoryTeamSpecification _sameShiftCategoryTeamSpecification;
		private readonly ISameShiftBlockSpecification _sameShiftBlockSpecification;
		private readonly ITeamBlockOpenHoursValidator _teamBlockOpenHoursValidator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public TeamBlockSteadyStateValidator(ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
											 ISameStartTimeBlockSpecification sameStartTimeBlockSpecification,
											 ISameStartTimeTeamSpecification sameStartTimeTeamSpecification,
											 ISameEndTimeTeamSpecification sameEndTimeTeamSpecification,
											 ISameShiftCategoryBlockSpecification sameShiftCategoryBlockSpecification,
											 ISameShiftCategoryTeamSpecification sameShiftCategoryTeamSpecification,
											 ISameShiftBlockSpecification sameShiftBlockSpecification,
											 ITeamBlockOpenHoursValidator teamBlockOpenHoursValidator,
											 Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_sameStartTimeBlockSpecification = sameStartTimeBlockSpecification;
			_sameStartTimeTeamSpecification = sameStartTimeTeamSpecification;
			_sameEndTimeTeamSpecification = sameEndTimeTeamSpecification;
			_sameShiftCategoryBlockSpecification = sameShiftCategoryBlockSpecification;
			_sameShiftCategoryTeamSpecification = sameShiftCategoryTeamSpecification;
			_sameShiftBlockSpecification = sameShiftBlockSpecification;
			_teamBlockOpenHoursValidator = teamBlockOpenHoursValidator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public bool IsTeamBlockInSteadyState(ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions)
		{
			bool isSteadyState = true;
			if (teamBlockInfo.TeamInfo.GroupMembers.Count() < 2 && teamBlockInfo.BlockInfo.BlockPeriod.DayCount() < 2)
				return isSteadyState;

			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(schedulingOptions))
				isSteadyState &= _sameStartTimeBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(schedulingOptions))
				isSteadyState &= _sameStartTimeTeamSpecification.IsSatisfiedBy(teamBlockInfo);
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(schedulingOptions))
				isSteadyState &= _sameShiftCategoryBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(schedulingOptions))
				isSteadyState &= _sameShiftCategoryTeamSpecification.IsSatisfiedBy(teamBlockInfo);
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(schedulingOptions))
			{
				isSteadyState &= _sameShiftBlockSpecification.IsSatisfiedBy(teamBlockInfo);
				isSteadyState &= _teamBlockOpenHoursValidator.Validate(teamBlockInfo, _schedulingResultStateHolder());
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(schedulingOptions))
				isSteadyState &= _sameEndTimeTeamSpecification.IsSatisfiedBy(teamBlockInfo);
			
			return isSteadyState;
		}
	}
}