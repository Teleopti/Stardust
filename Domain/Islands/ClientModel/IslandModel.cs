﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModel
	{
		public IslandModel()
		{
			SkillGroups = new List<SkillGroupModel>();
		}

		public ICollection<SkillGroupModel> SkillGroups { get; set; }
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