using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class OrderedSkillSets : IEnumerable<IEnumerable<CascadingSkillSet>>
	{
		private readonly IEnumerable<IEnumerable<CascadingSkillSet>> _orderedSkillSets;

		public OrderedSkillSets(IEnumerable<IEnumerable<CascadingSkillSet>> orderedSkillSets)
		{
			_orderedSkillSets = orderedSkillSets;
		}

		public IEnumerable<CascadingSkillSet> AllSkillSets()
		{
			return _orderedSkillSets.SelectMany(skillGroups => skillGroups);
		}

		public IEnumerator<IEnumerable<CascadingSkillSet>> GetEnumerator()
		{
			return _orderedSkillSets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}