using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence
{
	public class ApprovedPeriod
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}