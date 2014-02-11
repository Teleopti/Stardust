using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;

        public PostSwapValidationForTeamBlock(ISeniorityTeamBlockSwapValidator seniorityTeamBlockSwapValidator, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
        {
            _seniorityTeamBlockSwapValidator = seniorityTeamBlockSwapValidator;
            _teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
        }

        public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
        {
            if (!_seniorityTeamBlockSwapValidator.Validate(teamBlockInfo, optimizationPreferences))
                return false;

            if (!_teamBlockRestrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
                return false;

            return true;
        }
    }
}
