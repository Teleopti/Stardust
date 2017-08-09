using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class MoveSkillGroupToCorrectIsland
	{
		public bool Execute(ICollection<SkillGroup> allSkillGroups, IEnumerable<ICollection<SkillGroup>> islands)
		{
			var touchedIslands = new HashSet<ICollection<SkillGroup>>();

			foreach (var island in islands)
			{
				moveSkillGroupForOneIslandToCorrectIsland(allSkillGroups, islands, island, touchedIslands);
			}
			return touchedIslands.Any();
		}

		private static void moveSkillGroupForOneIslandToCorrectIsland(ICollection<SkillGroup> allSkillGroups, IEnumerable<ICollection<SkillGroup>> islands, ICollection<SkillGroup> island, ISet<ICollection<SkillGroup>> touchedIslands)
		{
			foreach (var skillGroup in island.ToArray())
			{
				var allOtherIslands = islands.Except(new[] { island });
				foreach (var otherIsland in allOtherIslands)
				{
					foreach (var otherSkillGroup in otherIsland.ToArray())
					{
						if (touchedIslands.Contains(island))
							return;

						if (otherSkillGroup.HasAnySkillSameAs(skillGroup))
						{
							if (skillGroup.HasSameSkillsAs(otherSkillGroup))
							{
								otherSkillGroup.AddAgentsFrom(skillGroup);
								allSkillGroups.Remove(skillGroup);
							}
							else
							{
								otherIsland.Add(skillGroup);
							}
							touchedIslands.Add(island);
							touchedIslands.Add(otherIsland);
							island.Remove(skillGroup);
						}
					}
				}
			}
		}
	}
}