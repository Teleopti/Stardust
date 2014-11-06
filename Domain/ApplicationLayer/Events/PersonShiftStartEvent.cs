using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonShiftStartEvent : IEvent
	{
		public Guid PersonId { get; set; }
	}
}