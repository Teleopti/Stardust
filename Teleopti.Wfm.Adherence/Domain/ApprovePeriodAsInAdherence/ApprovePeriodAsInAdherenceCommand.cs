using System;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherenceCommand
	{
		public Guid PersonId;
		public string StartDateTime;
		public string EndDateTime;
	}
}