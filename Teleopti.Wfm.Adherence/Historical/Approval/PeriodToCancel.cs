using System;

namespace Teleopti.Wfm.Adherence.Historical.Approval
{
	public class PeriodToCancel
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}