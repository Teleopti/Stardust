using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class DayOffTemplateChangedEvent : EventWithInfrastructureContext
	{
		public Guid DayOffTemplateId { get; set; }
		public string DayOffName { get; set; }
		public string DayOffShortName { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}
