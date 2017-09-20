using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class OrderedSkillSets : IEnumerable<IEnumerable<CascadingSkillSet>>
	{
		private readonly IEnumerable<IEnumerable<CascadingSkillSet>> _orderedSkillGroups;

		public OrderedSkillSets(IEnumerable<IEnumerable<CascadingSkillSet>> orderedSkillGroups)
		{
			_orderedSkillGroups = orderedSkillGroups;
		}

		public IEnumerable<CascadingSkillSet> AllSkillGroups()
		{
			return _orderedSkillGroups.SelectMany(skillGroups => skillGroups);
		}

		public IEnumerator<IEnumerable<CascadingSkillSet>> GetEnumerator()
		{
			return _orderedSkillGroups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}