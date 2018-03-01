using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IScheduleReader
	{
		IEnumerable<CurrentSchedule> Read();
		IEnumerable<CurrentSchedule> Read(int fromRevision);
	}

	public class CurrentSchedule
	{
		public Guid PersonId;
		public IEnumerable<ScheduledActivity> Schedule;
	}
}