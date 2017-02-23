using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class UnknownStateCodeReceviedEvent : IEvent
	{
		public Guid BusinessUnitId { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
	}
}