using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class PersonTeamChangedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public Guid? OldTeam { get; set; }
		public Guid? NewTeam { get; set; }
	}
}