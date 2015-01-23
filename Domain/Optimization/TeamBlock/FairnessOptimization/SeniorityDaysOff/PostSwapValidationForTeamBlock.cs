using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IPostSwapValidationForTeamBlock
    {
        bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
    }

    public class PostSwapValidationForTeamBlock : IPostSwapValidationForTeamBlock
    {
        private readonly ISeniorityTeamBlockSwapValidator _seniorityTeamBlockSwapValidator;
	    private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;

        public PostSwapValidationForTeamBlock(ISeniorityTeamBlockSwapValidator seniorityTeamBlockSwapValidator, ITeamBlockOptimizationLimits teamBlockOptimizationLimits)
        {
            _seniorityTeamBlockSwapValidator = seniorityTeamBlockSwapValidator;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
        }

        public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
        {
            if (!_seniorityTeamBlockSwapValidator.Validate(teamBlockInfo, optimizationPreferences))
                return false;

	        if (!_teamBlockOptimizationLimits.Validate(teamBlockInfo, optimizationPreferences))
		        return false;

	        if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamBlockInfo))
		        return false;

            return true;
        }
    }
}
