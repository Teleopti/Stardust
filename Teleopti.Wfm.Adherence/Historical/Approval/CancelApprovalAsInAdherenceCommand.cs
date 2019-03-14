using System;

namespace Teleopti.Wfm.Adherence.Historical.Approval
{
	public class CancelApprovalAsInAdherenceCommand
	{
		public Guid PersonId { get; set; }
		public string StartDateTime { get; set; }
		public string EndDateTime { get; set; }
	}
}