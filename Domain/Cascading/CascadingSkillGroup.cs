using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(IEnumerable<ISkill> primarySkills, IEnumerable<SubSkillsWithSameIndex> subSkillsWithSameIndex, double resources)
		{
			PrimarySkills = primarySkills;
			SubSkillsWithSameIndex = subSkillsWithSameIndex;
			Resources = resources;
			PrimarySkillsCascadingIndex = primarySkills.First().CascadingIndex.Value;
		}

		public int PrimarySkillsCascadingIndex { get; }
		public IEnumerable<ISkill> PrimarySkills { get; }
		public IEnumerable<SubSkillsWithSameIndex> SubSkillsWithSameIndex { get; }
		public double Resources { get; }
	}
}