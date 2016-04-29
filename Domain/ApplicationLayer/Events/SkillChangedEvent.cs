using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SkillNameChangedEvent : SkillChangedEvent
	{
	}

	public class SkillDeletedEvent : SkillChangedEvent
	{
	}

	// Base class for all skillchanged events
	public class SkillChangedEvent : EventWithInfrastructureContext
	{
		public Guid SkillId { get; set; }
	}
}