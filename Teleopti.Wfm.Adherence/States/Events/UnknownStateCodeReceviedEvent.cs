using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.States.Events
{
	public class UnknownStateCodeReceviedEvent : IEvent
	{
		public Guid BusinessUnitId { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
	}
}