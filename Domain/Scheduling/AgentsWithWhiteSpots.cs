using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AgentsWithWhiteSpots
	{
		public IEnumerable<IPerson> Execute(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var ret = new HashSet<IPerson>();
			foreach (var schedule in schedules.SchedulesForPeriod(period, agents.ToArray()))
			{
				if (!schedule.HasDayOff() && !schedule.PersonAssignment(true).ShiftLayers.Any())
				{
					ret.Add(schedule.Person);
				}
			}

			return ret;
		}
	}
}