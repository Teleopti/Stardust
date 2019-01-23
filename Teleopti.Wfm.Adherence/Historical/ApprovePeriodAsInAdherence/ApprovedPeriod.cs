using System;

namespace Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence
{
	public class ApprovedPeriod
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}