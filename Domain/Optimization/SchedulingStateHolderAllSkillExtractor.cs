using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts all the skills from a given stateHolder
    /// </summary>
    public class SchedulingStateHolderAllSkillExtractor : ISkillExtractor
    {
        private readonly Func<ISchedulingResultStateHolder> _stateHolder;

        public SchedulingStateHolderAllSkillExtractor(Func<ISchedulingResultStateHolder> stateHolder)
        {
            _stateHolder = stateHolder;
        }

        public IEnumerable<ISkill> ExtractSkills()
        {
            IList<ISkill> ret = new List<ISkill>();
	        foreach (var visibleSkill in _stateHolder().VisibleSkills)
	        {
		        if (visibleSkill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ret.Add(visibleSkill);
	        }

	        return ret;
        }
    }
}
