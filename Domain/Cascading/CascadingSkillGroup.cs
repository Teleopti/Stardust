using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(IEnumerable<ISkill> primarySkills, IEnumerable<SubSkillsWithSameIndex> subSkillsWithSameIndex, double resources)
		{
			PrimarySkills = primarySkills;
			SubSkillsWithSameIndex = subSkillsWithSameIndex;
			RemainingResources = resources;
			PrimarySkillsCascadingIndex = primarySkills.First().CascadingIndex.Value;
		}

		public int PrimarySkillsCascadingIndex { get; }
		public IEnumerable<ISkill> PrimarySkills { get; }
		public IEnumerable<SubSkillsWithSameIndex> SubSkillsWithSameIndex { get; }
		public double RemainingResources { get; set; }

		[RemoveMeWithToggle(Toggles.ResourcePlanner_NotShovelCorrectly_41763)]
		public string SkillGroupIndexHash()
		{
			var primaryHash = new StringBuilder();
			foreach (var primarySkill in PrimarySkills)
			{
				primaryHash.Append(primarySkill.CascadingIndex + "|");
			}
			return primaryHash.Append(subskillHash()).ToString();
		}

		public bool HasSameSkillGroupIndexAs(CascadingSkillGroup otherSkillGroup)
		{
			var thisPrimary = new HashSet<ISkill>(PrimarySkills);
			if (!thisPrimary.SetEquals(otherSkillGroup.PrimarySkills))
				return false;
			return subskillHash() == otherSkillGroup.subskillHash();
		}

		private string subskillHash()
		{
			var subHash = new StringBuilder();
			foreach (var subSkills in SubSkillsWithSameIndex)
			{
				subHash.Append("subindex:" + subSkills.CascadingIndex);
				foreach (var subSkill in subSkills)
				{
					subHash.Append("subsubindex:" + subSkill.CascadingIndex);
				}
			}
			return subHash.ToString();
		}
	}
}