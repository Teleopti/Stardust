using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SameSkillGroupSkillsComparer : IEqualityComparer<ICollection<ISkill>>
	{
		public bool Equals(ICollection<ISkill> x, ICollection<ISkill> y)
		{
			return x.All(y.Contains);
		}

		public int GetHashCode(ICollection<ISkill> obj)
		{
			return obj.Count;
		}
	}
}