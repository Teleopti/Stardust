using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class CalculateTimeZoneMessage : MessageWithLogOnContext
	{
		private readonly Guid _messageId = Guid.NewGuid();

		public override Guid Identity
		{
			get { return _messageId; }
		}

		public string TimeZoneCode { get; set; }
	}
}