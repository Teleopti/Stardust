using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class WebSchedulingSetupResult
	{
		public WebSchedulingSetupResult(PeopleSelection peopleSelection, IList<IScheduleDay> allSchedules)
		{
			PeopleSelection = peopleSelection;
			AllSchedules = allSchedules;
		}

		public PeopleSelection PeopleSelection { get; private set; }
		public IList<IScheduleDay> AllSchedules { get; private set; }
	}
}