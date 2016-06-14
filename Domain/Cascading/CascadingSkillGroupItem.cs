using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroupItem
	{
		private readonly IList<ISkill> _skills;

		public CascadingSkillGroupItem()
		{
			_skills = new List<ISkill>();
		}

		public IEnumerable<ISkill> Skills => _skills;

		public int NumberOfSkills { get; private set; }
		public int CascadingIndex { get; private set; }

		public void AddSkill(ISkill skill)
		{
			_skills.Add(skill);
			NumberOfSkills++;
			CascadingIndex = skill.CascadingIndex.Value;
		}
	}
}