﻿using System;
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
		public int TimeToGenerateInSeconds { get; set; }
	}

	public class IslandModel
	{
		public IEnumerable<SkillGroupModel> SkillGroups { get; set; }
		public int NumberOfAgentsOnIsland { get; set; }
	}

	public class SkillGroupModel : IComparable<SkillGroupModel>
	{
		public SkillGroupModel()
		{
			Skills = new List<SkillModel>();
		}

		public ICollection<SkillModel> Skills { get; set; }
		public int NumberOfAgentsOnSkillGroup { get; set; }

		public int CompareTo(SkillGroupModel other)
		{
			return other.NumberOfAgentsOnSkillGroup - NumberOfAgentsOnSkillGroup;
		}
	}

	public class SkillModel
	{
		public string Name { get; set; }
		public int NumberOfAgentsOnSkill { get; set; }
		public string ActivityName { get; set; }
	}
}