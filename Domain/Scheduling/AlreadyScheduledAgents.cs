using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AlreadyScheduledAgents
	{
		public IDictionary<IPerson, IEnumerable<DateOnly>> Execute(IScheduleDictionary schedules, DateOnlyPeriod period, IEnumerable<IPerson> agents)
		{
			var ret = new Dictionary<IPerson, IEnumerable<DateOnly>>();
			
			foreach (var scheduleDay in schedules.SchedulesForPeriod(period, agents.ToArray()))
			{
				if (scheduleDay.PersonAssignment(true).MainActivities().Any() || scheduleDay.HasDayOff())
				{
					if (!ret.TryGetValue(scheduleDay.Person, out var agentDates))
					{
						agentDates = new List<DateOnly>();
						ret[scheduleDay.Person] = agentDates;
					}
					((IList<DateOnly>)agentDates).Add(scheduleDay.DateOnlyAsPeriod.DateOnly);					
				}
			}

			return ret;
		}
	}
}