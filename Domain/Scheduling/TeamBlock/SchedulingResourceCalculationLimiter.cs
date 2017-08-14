using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SchedulingResourceCalculationLimiter
	{
		private readonly IDictionary<int, Percent> limits = new SortedDictionary<int, Percent>();

		public SchedulingResourceCalculationLimiter()
		{
			//add test and limits later...
		}

		public Percent Limit(int sizeOfSkillGroup)
		{
			var limit = new Percent(1);
			foreach (var keyValue in limits)
			{
				if (sizeOfSkillGroup >= keyValue.Key)
				{
					limit = keyValue.Value;
				}
				else
				{
					break;
				}
			}
			return limit;
		}

		public void SetLimits_UseOnlyFromTest(int limit, Percent percent)
		{
			limits.Clear();
			limits.Add(limit, percent);
		}
	}
}