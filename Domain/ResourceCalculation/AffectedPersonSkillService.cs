﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class AffectedPersonSkillService : IAffectedPersonSkillService
    {
        private readonly IEnumerable<ISkill> _skillCollection;

        public AffectedPersonSkillService(IEnumerable<ISkill> skillCollection)
        {
            InParameter.NotNull(nameof(skillCollection), skillCollection);
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
