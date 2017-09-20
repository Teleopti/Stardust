using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandTopModel
	{
		public IslandsModel AfterReducing { get; set; }
		public IslandsModel BeforeReducing { get; set; }
	}

	public class IslandsModel
	{
		public IEnumerable<IslandModel> Islands { get; set; }
		public int NumberOfAgentsOnAllIsland { get; set; }
		public int TimeToGenerateInMs { get; set; }
	}

	public class IslandModel : IComparable<IslandModel>
	{
		public IEnumerable<SkillSetModel> SkillSets { get; set; }
		public int NumberOfAgentsOnIsland { get; set; }

		public int CompareTo(IslandModel other)
		{
			return other.NumberOfAgentsOnIsland - NumberOfAgentsOnIsland;
		}
	}

	public class SkillSetModel : IComparable<SkillSetModel>
	{
		public IEnumerable<SkillModel> Skills { get; set; }
		public int NumberOfAgentsOnSkillSet { get; set; }

		public int CompareTo(SkillSetModel other)
		{
			return other.NumberOfAgentsOnSkillSet - NumberOfAgentsOnSkillSet;
		}
	}

	public class SkillModel : IComparable<SkillModel>
	{
		public string Name { get; set; }
		public int NumberOfAgentsOnSkill { get; set; }
		public string ActivityName { get; set; }

		public int CompareTo(SkillModel other)
		{
			return other.NumberOfAgentsOnSkill - NumberOfAgentsOnSkill;
		}
	}
}