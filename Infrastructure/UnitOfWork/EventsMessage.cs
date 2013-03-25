using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[Serializable]
	public class EventsMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();
		public override Guid Identity { get { return _messageId; } }

		public IEnumerable<IEvent> Events { get; set; }
	}
}