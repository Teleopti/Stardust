using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class InitiatorIdentifierFromMessage : IInitiatorIdentifier
	{
		public InitiatorIdentifierFromMessage(IInitiatorContext initiatorContext)
		{
			InitiatorId = initiatorContext.InitiatorId;
		}

		public Guid InitiatorId { get; set; }
	}
}