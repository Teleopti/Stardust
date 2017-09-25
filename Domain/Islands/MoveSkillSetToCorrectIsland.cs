using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class MoveSkillSetToCorrectIsland
	{
		public bool Execute(ICollection<SkillSet> allSkillSets, IEnumerable<ICollection<SkillSet>> islands)
		{
			var touchedIslands = new HashSet<ICollection<SkillSet>>();

			foreach (var island in islands)
			{
				moveSkillSetForOneIslandToCorrectIsland(allSkillSets, islands, island, touchedIslands);
			}
			return touchedIslands.Any();
		}

		private static void moveSkillSetForOneIslandToCorrectIsland(ICollection<SkillSet> allSkillSets, IEnumerable<ICollection<SkillSet>> islands, ICollection<SkillSet> island, ISet<ICollection<SkillSet>> touchedIslands)
		{
			foreach (var SkillSet in island.ToArray())
			{
				var allOtherIslands = islands.Except(new[] { island });
				foreach (var otherIsland in allOtherIslands)
				{
					foreach (var otherSkillSet in otherIsland.ToArray())
					{
						if (touchedIslands.Contains(island))
							return;

						if (otherSkillSet.HasAnySkillSameAs(SkillSet))
						{
							if (SkillSet.HasSameSkillsAs(otherSkillSet))
							{
								otherSkillSet.AddAgentsFrom(SkillSet);
								allSkillSets.Remove(SkillSet);
							}
							else
							{
								otherIsland.Add(SkillSet);
							}
							touchedIslands.Add(island);
							touchedIslands.Add(otherIsland);
							island.Remove(SkillSet);
						}
					}
				}
			}
		}
	}
}