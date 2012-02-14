using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts all the skills from a given stateHolder
    /// </summary>
    public class SchedulingStateHolderAllSkillExtractor : ISkillExtractor
    {
        private readonly ISchedulingResultStateHolder _stateHolder;

        public SchedulingStateHolderAllSkillExtractor(ISchedulingResultStateHolder stateHolder)
        {
            _stateHolder = stateHolder;
        }

        public IEnumerable<ISkill> ExtractSkills()
        {
            return _stateHolder.Skills;
        }
    }
}
