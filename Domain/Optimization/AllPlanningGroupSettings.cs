using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AllPlanningGroupSettings : IEnumerable<PlanningGroupSettings>
	{
		private readonly IEnumerable<PlanningGroupSettings> _planningGroupSettings;

		public AllPlanningGroupSettings(IEnumerable<PlanningGroupSettings> planningGroupSettings)
		{
			_planningGroupSettings = planningGroupSettings.OrderByDescending(x=>x.Priority).ToArray();
		}

		public PlanningGroupSettings ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _planningGroupSettings.FirstOrDefault(x => x.IsValidForAgent(person, dateOnly));
		}

		public IEnumerator<PlanningGroupSettings> GetEnumerator()
		{
			return _planningGroupSettings.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}