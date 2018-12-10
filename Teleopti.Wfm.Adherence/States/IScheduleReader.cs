using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
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