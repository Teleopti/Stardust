using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class AffectedPersonSkillService : IAffectedPersonSkillService
    {
        private readonly ICollection<ISkill> _skillCollection;

        public AffectedPersonSkillService(ICollection<ISkill> skillCollection)
        {
            InParameter.NotNull("validSkillCollection", skillCollection);
            _skillCollection = skillCollection;
        }

        public IEnumerable<ISkill> AffectedSkills
        {
            get
            {
				IList<ISkill> skills = new List<ISkill>();
				foreach (var affectedSkill in _skillCollection)
				{

					if (affectedSkill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill || affectedSkill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
						continue;
						
					skills.Add(affectedSkill);
				}
				return skills;
            }
        }
    }
}
