using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AlreadyScheduledAgents
	{
		public IDictionary<IPerson, HashSet<DateOnly>> Execute(IScheduleDictionary schedules, DateOnlyPeriod period, IEnumerable<IPerson> agents)
		{
			var ret = new Dictionary<IPerson, HashSet<DateOnly>>();
			
			foreach (var scheduleDay in schedules.SchedulesForPeriod(period, agents.ToArray()))
			{
				if (scheduleDay.PersonAssignment(true).MainActivities().Any() || scheduleDay.HasDayOff())
				{
					if (!ret.TryGetValue(scheduleDay.Person, out var agentDates))
					{
						agentDates = new HashSet<DateOnly>();
						ret.Add(scheduleDay.Person, agentDates);
					}
					agentDates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);					
				}
			}

			return ret;
		}
	}
}