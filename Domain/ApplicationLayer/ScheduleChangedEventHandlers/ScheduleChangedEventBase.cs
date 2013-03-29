using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public abstract class ScheduleChangedEventBase : RaptorDomainEvent
	{
		private readonly Guid _messageId = Guid.NewGuid();
		public override Guid Identity
		{
			get { return _messageId; }
		}

		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public bool SkipDelete { get; set; }
	}
}