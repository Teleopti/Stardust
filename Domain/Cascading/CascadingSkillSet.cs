using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillSet
	{
		public CascadingSkillSet(IEnumerable<ISkill> primarySkills, IEnumerable<SubSkillsWithSameIndex> subSkillsWithSameIndex, double resources)
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

		public bool HasSameSkillSetIndexAs(CascadingSkillSet otherSkillSet)
		{
			var thisPrimary = new HashSet<ISkill>(PrimarySkills);
			if (!thisPrimary.SetEquals(otherSkillSet.PrimarySkills))
				return false;
			return subskillHash() == otherSkillSet.subskillHash();
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