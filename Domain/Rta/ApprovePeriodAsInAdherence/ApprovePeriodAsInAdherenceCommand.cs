using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherenceCommand
	{
		public Guid PersonId;
		public string StartDateTime;
		public string EndDateTime;
	}
}