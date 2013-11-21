using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IValidateScheduleDays
    {
        bool Validate(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock);
    }

    public class ValidateScheduleDays : IValidateScheduleDays
    {
        public bool  Validate(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            return true;
        }
    }
}