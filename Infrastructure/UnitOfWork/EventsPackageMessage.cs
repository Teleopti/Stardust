using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[Serializable]
	public class EventsPackageMessage : MessageWithLogOnInfo
	{
		private readonly Guid _messageId = Guid.NewGuid();
		public override Guid Identity { get { return _messageId; } }

		public List<Event> Events { get; set; }
	}
}