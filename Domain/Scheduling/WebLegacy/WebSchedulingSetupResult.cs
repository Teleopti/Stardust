using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class WebSchedulingSetupResult
	{
		public WebSchedulingSetupResult(IList<IScheduleDay> allSchedules)
		{
			AllSchedules = allSchedules;
		}
		public IList<IScheduleDay> AllSchedules { get; private set; }
	}
}