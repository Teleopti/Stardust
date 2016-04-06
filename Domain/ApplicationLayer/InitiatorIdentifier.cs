using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class InitiatorIdentifier : IInitiatorIdentifier
	{
		public static IInitiatorIdentifier FromMessage(object message)
		{
			var initiatorInfo = message as IInitiatorContext;
			return initiatorInfo != null
				? new InitiatorIdentifier {InitiatorId = initiatorInfo.InitiatorId}
				: null;
		}

		public Guid InitiatorId { get; set; }
	}
}