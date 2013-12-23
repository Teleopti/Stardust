using System;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class InitiatorIdentifierFromMessage : IInitiatorIdentifier
	{
		public InitiatorIdentifierFromMessage(IRaptorDomainMessageInfo raptorDomainMessage)
		{
			InitiatorId = raptorDomainMessage.InitiatorId;
		}

		public Guid InitiatorId { get; set; }
	}
}