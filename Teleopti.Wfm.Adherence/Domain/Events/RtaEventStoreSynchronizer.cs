using System;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizer
	{
		void Synchronize();
	}


	public class NoRtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		public void Synchronize()
		{
		}
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;

		public const string SynchronizedEventKey = "HistoricalOverviewReadModelSynchronizedEvent";
		
		public RtaEventStoreSynchronizer(
			IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader,
			IKeyValueStorePersister keyValueStore)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
			_keyValueStore = keyValueStore;
		}

		[FullPermissions]
		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void Synchronize()
		{
			var events = _events.LoadFrom(_keyValueStore.Get(SynchronizedEventKey, 0));

			_keyValueStore.Update(SynchronizedEventKey, events.MaxId.ToString());

			events.Events
				.Cast<IRtaStoredEvent>()
				.GroupBy(e => new {e.QueryData().PersonId, Day = e.QueryData().StartTime.Value.ToDateOnly()})
				.ForEach(personAndDay => { synchronize(personAndDay.Key.PersonId.Value, personAndDay.Key.Day); });
		}

		private void synchronize(Guid personId, DateOnly day)
		{
			var adherenceDay = _adherenceDayLoader.Load(personId, day);
			var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWork != null);
			var lateForWorkText = lateForWork != null ? lateForWork.LateForWork : "0";
			var minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""));
			
			_readModels.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = day,
				Adherence = adherenceDay.Percentage(),
				WasLateForWork = lateForWork != null,
				MinutesLateForWork = minutesLateForWork,
				SecondsInAdherence = adherenceDay.SecondsInAherence(),
				SecondsOutOfAdherence = adherenceDay.SecondsOutOfAdherence(),
			});
		}
	}
}