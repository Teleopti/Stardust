using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	public class CalculateBadgeMessage : RaptorDomainMessage
	{
		public override Guid Identity
		{
			get { return Guid.NewGuid(); }
		}

		public DateOnly CalculationDate { get; set; }
		public TimeZoneInfo TimeZone { get; set; }
	}
}