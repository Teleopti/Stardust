using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SameSkillGroupSkillsComparer : IEqualityComparer<ISet<ISkill>>
	{
		public bool Equals(ISet<ISkill> x, ISet<ISkill> y)
		{
			return x.SetEquals(y);
		}

		public int GetHashCode(ISet<ISkill> obj)
		{
			return obj.Count;
		}
	}
}