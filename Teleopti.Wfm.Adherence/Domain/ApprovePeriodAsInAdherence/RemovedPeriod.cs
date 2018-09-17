using System;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemovedPeriod
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}