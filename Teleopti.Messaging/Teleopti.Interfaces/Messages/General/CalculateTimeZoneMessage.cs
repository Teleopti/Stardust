using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class CalculateTimeZoneMessage : MessageWithLogOnInfo
	{
		public override Guid Identity
		{
			get { return Guid.NewGuid(); }
		}

		public string TimeZoneCode { get; set; }
	}
}