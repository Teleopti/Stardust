using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonRuleChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }


	}
}