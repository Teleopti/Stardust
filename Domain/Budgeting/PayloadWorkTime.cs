using System;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class PayloadWorkTime
	{
		public Guid PayloadId { get; set; }
		public long TotalContractTime { get; set; }
		public DateTime BelongsToDate { get; set; }
		public int HeadCounts { get; set; }
	}
}