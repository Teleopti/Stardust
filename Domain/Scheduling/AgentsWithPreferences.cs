using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AgentsWithPreferences
	{
		public IEnumerable<IPerson> Execute(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var ret = new HashSet<IPerson>();
			foreach (var schedule in schedules.SchedulesForPeriod(period, agents.ToArray()))
			{
				if (schedule.PreferenceDay() != null)
					ret.Add(schedule.Person);
			}

			return ret;
		}
	}
}