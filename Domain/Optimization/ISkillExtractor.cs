using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts the list of skills
    /// </summary>
    public interface ISkillExtractor
    {
        IEnumerable<ISkill> ExtractSkills();
    }
}