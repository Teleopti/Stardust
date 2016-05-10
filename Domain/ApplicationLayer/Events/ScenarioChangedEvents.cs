using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ScenarioAddEvent : ScenarioEventBase
	{
	}

	public class ScenarioNameChangeEvent : ScenarioEventBase
	{
		public string ScenarioName { get; set; }
	}

	public abstract class ScenarioEventBase : EventWithInfrastructureContext
	{
		public Guid ScenarioId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}
