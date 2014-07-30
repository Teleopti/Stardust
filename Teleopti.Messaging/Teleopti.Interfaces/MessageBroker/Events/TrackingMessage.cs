using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public class TrackingMessage
	{
		public Guid TrackId { get; set; }
		public TrackingMessageStatus Status { get; set; }
	}

	public enum TrackingMessageStatus
	{
		Success = 0,
		Fail = 1
	}
}