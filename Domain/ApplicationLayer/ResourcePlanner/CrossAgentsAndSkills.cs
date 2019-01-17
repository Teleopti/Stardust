using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class CrossAgentsAndSkills
	{
		public CrossAgentsAndSkillsResult Execute(IEnumerable<Island> allIslands, IEnumerable<IPerson> selectedAgents)
		{
			var retAgents = new HashSet<Guid>();
			var retSkills = new HashSet<Guid>();
			foreach (var selectedAgent in selectedAgents)
			{
				foreach (var island in allIslands)
				{
					var islandAgents = island.AgentsInIsland().ToHashSet();
					if (islandAgents.Contains(selectedAgent))
					{
						foreach (var agentId in islandAgents.Select(x => x.Id.Value))
						{
							retAgents.Add(agentId);
						}
						foreach (var skillId in island.SkillIds())
						{
							retSkills.Add(skillId);
						}
						break;
					}
				}
			}
			return new CrossAgentsAndSkillsResult(retAgents, retSkills);
		}
	}
}