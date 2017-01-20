using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class ScheduleOvertimeWithoutStateHolder
	{
		public void Execute(IEnumerable<IScheduleDay> scheduleDays)
		{
			if (!scheduleDays.Any()) return;
			throw new System.NotImplementedException();
		}
	}
}
