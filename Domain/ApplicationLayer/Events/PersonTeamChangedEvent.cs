using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTeamChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public Guid? OldTeam { get; set; }
		public Guid? NewTeam { get; set; }
	}
}