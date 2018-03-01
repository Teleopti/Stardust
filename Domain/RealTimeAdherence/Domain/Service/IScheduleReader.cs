using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
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