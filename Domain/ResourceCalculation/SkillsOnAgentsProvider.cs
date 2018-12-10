using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillsOnAgentsProvider
	{
		private readonly PersonalSkills _personalSkills;

		public SkillsOnAgentsProvider(PersonalSkills personalSkills)
		{
			_personalSkills = personalSkills;
		}
		
		public IEnumerable<ISkill> Execute(IEnumerable<IPerson> loadedAgents, DateOnlyPeriod period)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var agent in loadedAgents)
			{
				var pPeriod = agent.Period(period.StartDate);
				if (pPeriod != null)
				{
					_personalSkills.PersonSkills(pPeriod).ForEach(x => agentSkills.Add(x.Skill));					
				}
			}

			return agentSkills;
		}

	}
}