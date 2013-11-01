using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockSteadyStateValidator
    {
        bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);
    }

    public class TeamBlockSteadyStateValidator : ITeamBlockSteadyStateValidator
    {
        private readonly ITeamBlockSameEndTimeSpecification _teamBlockSameEndTimeSpecification;
        private readonly ITeamBlockSameShiftCategorySpecification _teamBlockSameShiftCategorySpecification;
        private readonly ITeamBlockSameShiftSpecification _teamBlockSameShiftSpecification;
        private readonly ITeamBlockSameStartTimeSpecification _teamBlockSameStatTimeSpecification;

        public TeamBlockSteadyStateValidator(ITeamBlockSameStartTimeSpecification teamBlockSameStatTimeSpecification,
                                             ITeamBlockSameEndTimeSpecification teamBlockSameEndTimeSpecification,
                                             ITeamBlockSameShiftCategorySpecification
                                                 teamBlockSameShiftCategorySpecification,
                                             ITeamBlockSameShiftSpecification teamBlockSameShiftSpecification)
        {
            _teamBlockSameStatTimeSpecification = teamBlockSameStatTimeSpecification;
            _teamBlockSameEndTimeSpecification = teamBlockSameEndTimeSpecification;
            _teamBlockSameShiftCategorySpecification = teamBlockSameShiftCategorySpecification;
            _teamBlockSameShiftSpecification = teamBlockSameShiftSpecification;
        }

        public bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
        {
            if (schedulingOptions.UseTeamBlockSameStartTime)
                return _teamBlockSameStatTimeSpecification.IsSatisfiedBy(teamBlockInfo);
            if (schedulingOptions.UseTeamBlockSameEndTime)
                return _teamBlockSameEndTimeSpecification.IsSatisfiedBy(teamBlockInfo);
            if (schedulingOptions.UseTeamBlockSameShift)
                return _teamBlockSameShiftCategorySpecification.IsSatisfiedBy(teamBlockInfo);
            if (schedulingOptions.UseTeamBlockSameShiftCategory)
                return _teamBlockSameShiftSpecification.IsSatisfiedBy(teamBlockInfo);
            return true;
        }
    }
}