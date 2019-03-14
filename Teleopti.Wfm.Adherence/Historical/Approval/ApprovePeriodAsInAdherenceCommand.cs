using System;

namespace Teleopti.Wfm.Adherence.Historical.Approval
{
	public class ApprovePeriodAsInAdherenceCommand
	{
		public Guid PersonId;
		public string StartDateTime;
		public string EndDateTime;
	}
}