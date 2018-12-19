using System;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class TrackingMessage
	{
		public Guid TrackId { get; set; }
		public TrackingMessageStatus Status { get; set; }
	}

	public enum TrackingMessageStatus
	{
		Success = 1,
		Failed = 2
	}
}