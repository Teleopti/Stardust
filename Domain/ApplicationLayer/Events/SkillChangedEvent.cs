using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SkillChangedEvent : EventWithInfrastructureContext
	{
		public Guid SkillId { get; set; }
	}
}