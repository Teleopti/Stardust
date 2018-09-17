using System;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriodCommand
	{
		public Guid PersonId { get; set; }
		public string StartDateTime { get; set; }
		public string EndDateTime { get; set; }
	}
}