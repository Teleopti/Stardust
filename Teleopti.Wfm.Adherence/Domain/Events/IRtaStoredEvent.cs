using System;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaStoredEvent
	{
		QueryData QueryData();
	}

	public class QueryData
	{
		public Guid? PersonId;
		public DateOnly? BelongsToDate;
		public DateTime? StartTime;
		public DateTime? EndTime;
	}
}