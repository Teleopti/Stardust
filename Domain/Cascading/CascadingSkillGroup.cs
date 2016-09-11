using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

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

		public string SkillGroupIndexHash()
		{
			var hash = new StringBuilder();
			foreach (var primarySkill in PrimarySkills)
			{
				hash.Append(primarySkill.CascadingIndex + "|");
			}
			foreach (var subSkills in SubSkillsWithSameIndex)
			{
				hash.Append("subindex:" + subSkills.CascadingIndex);
				foreach (var subSkill in subSkills)
				{
					hash.Append(subSkill.CascadingIndex);
				}
			}
			return hash.ToString();
		}
	}
}