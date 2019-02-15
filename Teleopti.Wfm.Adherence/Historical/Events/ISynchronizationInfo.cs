using System;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public interface ISynchronizationInfo
	{
		SynchronizationInfo SynchronizationInfo();
	}

	public class SynchronizationInfo
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime StartTime { get; set; }
	}
}