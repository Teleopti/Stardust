using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTeamChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public Guid? CurrentBusinessUnitId { get; set; }
		public Guid? CurrentSiteId { get; set; }
		public Guid? CurrentTeamId { get; set; }
	}
}