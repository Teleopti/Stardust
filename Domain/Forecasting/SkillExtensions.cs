using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillExtensions
	{
		public static bool HasSameOpenHours(this IEnumerable<ISkill> skills)
		{
			//now returns true only if openhourlist is exactly the same. Could be "optimized" and look at _actual_ open hours intead...
			
			IEnumerable<TimePeriod> openHourList=null;
			foreach (var skill in skills)
			{
				foreach (var workload in skill.WorkloadCollection)
				{
					foreach (var dayTemplate in workload.TemplateWeekCollection.Values)
					{
						if (openHourList == null)
						{
							openHourList = dayTemplate.OpenHourList;
						}
						else
						{
							if (!openHourList.SequenceEqual(dayTemplate.OpenHourList))
							{
								return false;
							}
						}
					}
				}
			}

			return true;
		}
	}
}