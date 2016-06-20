using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SkillDayChangedEvent : EventWithInfrastructureContext
	{
		public Guid SkillDayId { get; set; }
	}
}