using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class SkillDaySameDateAndSkillComparer : IEqualityComparer<ISkillDay>
	{
		public bool Equals(ISkillDay x, ISkillDay y)
		{
			return x.CurrentDate == y.CurrentDate && x.Skill.Equals(y.Skill);
		}

		public int GetHashCode(ISkillDay obj)
		{
			return obj.Skill.GetHashCode() ^ obj.CurrentDate.GetHashCode();
		}
	}
}