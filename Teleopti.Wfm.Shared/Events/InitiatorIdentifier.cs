using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class InitiatorIdentifier : IInitiatorIdentifier
	{
		public static IInitiatorIdentifier FromMessage(object message)
		{
			return message is IInitiatorContext initiatorInfo
				? new InitiatorIdentifier {InitiatorId = initiatorInfo.InitiatorId}
				: null;
		}

		public Guid InitiatorId { get; set; }
	}
}