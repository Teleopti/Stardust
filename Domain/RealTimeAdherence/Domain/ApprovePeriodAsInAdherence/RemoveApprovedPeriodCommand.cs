using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriodCommand
	{
		public Guid PersonId { get; set; }
		public string StartDateTime { get; set; }
		public string EndDateTime { get; set; }
	}
}