using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class NoRtaEventStore : IRtaEventStore
	{
		public void Add(IEvent @event)
		{
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			return Enumerable.Empty<IEvent>();
		}

		public IEvent LoadLastBefore(Guid personId, DateTime timestamp)
		{
			return null;
		}
	}
}