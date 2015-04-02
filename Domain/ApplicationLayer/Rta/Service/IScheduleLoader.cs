using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IScheduleLoader
	{
		IEnumerable<ScheduleLayer> GetCurrentSchedule(Guid personId);
	}
}