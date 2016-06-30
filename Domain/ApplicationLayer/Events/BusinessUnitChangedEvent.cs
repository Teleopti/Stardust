using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class BusinessUnitChangedEvent : IEvent
	{
		public Guid BusinessUnitId { get; set; }
		public string BusinessUnitName { get; set; }
		public DateTime UpdatedOn { get; set; }
	}
}