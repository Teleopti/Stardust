using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroupItem
	{
		private readonly IList<ISkill> _subSkills;

		public CascadingSkillGroupItem()
		{
			_subSkills = new List<ISkill>();
		}

		public IEnumerable<ISkill> SubSkills => _subSkills;

		public int NumberOfSkills { get; private set; }
		public int CascadingIndex { get; private set; }

		public void AddSubSkill(ISkill subSkill)
		{
			_subSkills.Add(subSkill);
			NumberOfSkills++;
			CascadingIndex = subSkill.CascadingIndex.Value;
		}
	}
}