using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class CalculateTimeZoneMessage : RaptorDomainMessage
	{
		public override Guid Identity
		{
			get { return Guid.NewGuid(); }
		}

		public TimeZoneInfo TimeZone { get; set; }
	}
}