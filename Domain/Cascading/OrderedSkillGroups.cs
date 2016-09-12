using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class OrderedSkillGroups : IEnumerable<IEnumerable<CascadingSkillGroup>>
	{
		private readonly IEnumerable<IEnumerable<CascadingSkillGroup>> _orderedSkillGroups;

		public OrderedSkillGroups(IEnumerable<IEnumerable<CascadingSkillGroup>> orderedSkillGroups)
		{
			_orderedSkillGroups = orderedSkillGroups;
		}

		public IEnumerable<CascadingSkillGroup> AllSkillGroups()
		{
			return _orderedSkillGroups.SelectMany(skillGroups => skillGroups);
		}

		public IEnumerator<IEnumerable<CascadingSkillGroup>> GetEnumerator()
		{
			return _orderedSkillGroups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}