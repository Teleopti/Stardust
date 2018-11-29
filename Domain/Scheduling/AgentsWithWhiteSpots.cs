using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AgentsWithWhiteSpots
	{
		public IEnumerable<IPerson> Execute(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var ret = new HashSet<IPerson>();
			foreach (var schedule in schedules.SchedulesForPeriod(period, agents.ToArray()))
			{
				if(ret.Contains(schedule.Person))
					continue;
				if (!schedule.HasDayOff() && !schedule.ProjectionService().CreateProjection().HasLayers)
				{
					ret.Add(schedule.Person);
				}
			}
			return ret;
		}
	}
}