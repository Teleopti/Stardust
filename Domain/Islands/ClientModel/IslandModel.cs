using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandTopModel
	{
		public IEnumerable<IslandModel> AfterReducing { get; set; }
		public IEnumerable<IslandModel> BeforeReducing { get; set; }
	}

	public class IslandModel
	{
		public IslandModel()
		{
			SkillGroups = new List<SkillGroupModel>();
		}

		public ICollection<SkillGroupModel> SkillGroups { get; set; }
		public int NumberOfAgentsOnIsland { get; set; }
	}

	public class SkillGroupModel
	{
		public SkillGroupModel()
		{
			Skills = new List<SkillModel>();
		}

		public ICollection<SkillModel> Skills { get; set; }
		public int NumberOfAgentsOnSkillGroup { get; set; }
	}

	public class SkillModel
	{
		public string Name { get; set; }
		public int NumberOfAgentsOnSkill { get; set; }
	}
}