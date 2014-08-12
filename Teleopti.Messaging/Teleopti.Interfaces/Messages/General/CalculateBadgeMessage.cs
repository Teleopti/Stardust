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

		public DateTime CalculationDate { get; set; }
		public string TimeZoneCode { get; set; }
	}
}