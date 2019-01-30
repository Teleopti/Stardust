using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ForecastChangedEvent : EventWithInfrastructureContext
	{
		public Guid[] SkillDayIds { get; set; }
	}
}