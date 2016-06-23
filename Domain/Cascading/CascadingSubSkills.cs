using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSubSkills : IEnumerable<ISkill>
	{
		private readonly IList<ISkill> _subSkills;

		public CascadingSubSkills()
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