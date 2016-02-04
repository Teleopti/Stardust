using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	public class CalculateBadgeMessage : MessageWithLogOnContext
	{
		private readonly Guid _messageId = Guid.NewGuid();

		public override Guid Identity
		{
			get { return _messageId; }
		}

		public DateTime CalculationDate { get; set; }
		public string TimeZoneCode { get; set; }
	}
}