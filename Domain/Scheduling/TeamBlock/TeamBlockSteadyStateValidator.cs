using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSteadyStateValidator
	{
		bool IsTeamBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);
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

		public TeamBlockSteadyStateValidator(ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
											 ISameStartTimeBlockSpecification sameStartTimeBlockSpecification,
											 ISameStartTimeTeamSpecification sameStartTimeTeamSpecification,
											 ISameEndTimeTeamSpecification sameEndTimeTeamSpecification,
											 ISameShiftCategoryBlockSpecification sameShiftCategoryBlockSpecification,
											 ISameShiftCategoryTeamSpecification sameShiftCategoryTeamSpecification,
											 ISameShiftBlockSpecification sameShiftBlockSpecification)
		{
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_sameStartTimeBlockSpecification = sameStartTimeBlockSpecification;
			_sameStartTimeTeamSpecification = sameStartTimeTeamSpecification;
			_sameEndTimeTeamSpecification = sameEndTimeTeamSpecification;
			_sameShiftCategoryBlockSpecification = sameShiftCategoryBlockSpecification;
			_sameShiftCategoryTeamSpecification = sameShiftCategoryTeamSpecification;
			_sameShiftBlockSpecification = sameShiftBlockSpecification;
		}

		public bool IsTeamBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
		{
			bool isSteadyState = true;
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
				isSteadyState &= _sameShiftBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(schedulingOptions))
				isSteadyState &= _sameEndTimeTeamSpecification.IsSatisfiedBy(teamBlockInfo);
			return isSteadyState;
		}
	}
}