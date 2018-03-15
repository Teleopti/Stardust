using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class NoRtaEventStore : IRtaEventStore, IRtaEventStoreReader
	{
		public void Add(IEvent @event)
		{
		}

		public void Remove(DateTime removeUntil)
		{
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			return Enumerable.Empty<IEvent>();
		}

		public IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp)
		{
			return null;
		}
	}
}