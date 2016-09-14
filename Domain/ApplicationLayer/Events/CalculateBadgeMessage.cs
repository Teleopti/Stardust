using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class CalculateBadgeMessage
	{
		public Guid LogOnBusinessUnitId { get; set; }
		public DateTime CalculationDate { get; set; }
		public string TimeZoneCode { get; set; }
		public int? TimeoutInSeconds { get; set; }
	}
}