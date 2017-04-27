using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class AffectedPersonSkillService : IAffectedPersonSkillService
    {
	    private readonly IEnumerable<ISkill> _affectedSkills;

	    public AffectedPersonSkillService(IEnumerable<ISkill> skillCollection)
        {
            InParameter.NotNull(nameof(skillCollection), skillCollection);
	        _affectedSkills = skillCollection.Where(x => x.SkillType.ForecastSource != ForecastSource.MaxSeatSkill &&
														 x.SkillType.ForecastSource != ForecastSource.NonBlendSkill);
        }

	    public IEnumerable<ISkill> AffectedSkills => _affectedSkills.ToArray();
    }
}
