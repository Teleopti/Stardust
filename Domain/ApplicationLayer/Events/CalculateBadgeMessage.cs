using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class CalculateBadgeMessage : EventWithInfrastructureContext
	{
		private readonly Guid _messageId = Guid.NewGuid();

		public Guid Identity
		{
			get { return _messageId; }
		}

		public DateTime CalculationDate { get; set; }
		public string TimeZoneCode { get; set; }
	}
}