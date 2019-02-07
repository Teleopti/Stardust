using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillsExtensions
	{
		public static int MinimumSkillIntervalLength(this IEnumerable<ISkill> skills)
		{
			return skills.Any() ? skills.Min(s => s.DefaultResolution) : 15;
		}
	}
}