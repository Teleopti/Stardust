using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class AffectedPersonSkillService
    {
	    public AffectedPersonSkillService(IEnumerable<ISkill> skillCollection)
        {
            InParameter.NotNull(nameof(skillCollection), skillCollection);
	        AffectedSkills = skillCollection.Where(x => x.SkillType.ForecastSource != ForecastSource.MaxSeatSkill &&
														 x.SkillType.ForecastSource != ForecastSource.NonBlendSkill).ToArray();
			ActivityLookup = AffectedSkills.ToLookup(s => s.Activity);
		}

	    public IEnumerable<ISkill> AffectedSkills { get; }

		public ILookup<IActivity,ISkill> ActivityLookup { get; }
    }
}
