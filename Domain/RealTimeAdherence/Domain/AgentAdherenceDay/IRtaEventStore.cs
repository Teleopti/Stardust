using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IRtaEventStore
	{
		void Add(IEvent @event);
		void Remove(DateTime removeUntil);
	}

	public interface IRtaEventStoreReader
	{
		IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period);
		IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp);
	}

	public interface IRtaEventStoreTestReader
	{
		IEnumerable<IEvent> LoadAll();
	}
}