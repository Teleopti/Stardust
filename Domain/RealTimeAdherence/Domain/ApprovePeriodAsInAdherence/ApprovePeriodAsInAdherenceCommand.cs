using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherenceCommand
	{
		public Guid PersonId;
		public string StartDateTime;
		public string EndDateTime;
	}
}