using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SubSkillsWithSameIndex : IEnumerable<ISkill>
	{
		private readonly IList<ISkill> _subSkills;

		public SubSkillsWithSameIndex()
		{
			_subSkills = new List<ISkill>();
		}

		public int CascadingIndex { get; private set; }

		public void AddSubSkill(ISkill subSkill)
		{
			_subSkills.Add(subSkill);
			CascadingIndex = subSkill.CascadingIndex.Value;
		}

		public IEnumerator<ISkill> GetEnumerator()
		{
			return _subSkills.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}