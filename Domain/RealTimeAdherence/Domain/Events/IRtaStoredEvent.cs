using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public interface IRtaStoredEvent
	{
		QueryData QueryData();
	}

	public class QueryData
	{
		public Guid? PersonId;
		public DateTime? StartTime;
		public DateTime? EndTime;
	}
}