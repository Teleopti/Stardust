using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizer
	{
		void Synchronize();
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IPersonRepository _persons;

		public const string SynchronizedEventKey = "HistoricalOverviewReadModelSynchronizedEvent";

		public RtaEventStoreSynchronizer(
			IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader,
			IKeyValueStorePersister keyValueStore, IPersonRepository persons)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
			_keyValueStore = keyValueStore;
			_persons = persons;
		}

		public void Synchronize()
		{
			var fromEventId = SynchronizedEventId();
			var events = LoadEvents(fromEventId);
			Synchronize(events.Events);
			UpdateSynchronizedEventId(events.MaxId);
		}

		[UnitOfWork]
		protected virtual LoadedEvents LoadEvents(int fromEventId) =>
			_events.LoadFrom(fromEventId);

		[ReadModelUnitOfWork]
		protected virtual void UpdateSynchronizedEventId(int toEventId) =>
			_keyValueStore.Update(SynchronizedEventKey, toEventId.ToString());

		[ReadModelUnitOfWork]
		protected virtual int SynchronizedEventId() =>
			_keyValueStore.Get(SynchronizedEventKey, 0);

		[AllBusinessUnitsUnitOfWork]
		[FullPermissions]
		protected virtual void Synchronize(IEnumerable<IEvent> events)
		{
			var toBeSynched = events
				.Cast<IRtaStoredEvent>()
				.Select(e =>
				{
					var data = e.QueryData();
					return new
					{
						PersonId = data.PersonId.Value,
						Day = data.StartTime.Value.ToDateOnly()
					};
				})
				.Distinct()
				.ToArray();

			var personsTimeZones = _persons.FindPeople(toBeSynched.Select(x => x.PersonId).Distinct())
				.ToDictionary(k => k.Id.GetValueOrDefault(), v => v.PermissionInformation.DefaultTimeZone());

			toBeSynched.ForEach(x =>
			{
				if (!personsTimeZones.TryGetValue(x.PersonId, out var personTimeZone))
					personTimeZone = TimeZoneInfo.Utc;

				var shouldSynchPreviousDay = toBeSynched
												 .FirstOrDefault(
													 y => y.PersonId == x.PersonId &&
														  y.Day == x.Day.AddDays(-1)) == null;

				if (shouldSynchPreviousDay)
					synchronizeAdherenceDay(x.PersonId, x.Day.AddDays(-1), personTimeZone);
				synchronizeAdherenceDay(x.PersonId, x.Day, personTimeZone);
			});
		}

		private void synchronizeAdherenceDay(Guid personId, DateOnly day, TimeZoneInfo timeZone)
		{
			var adherenceDay = _adherenceDayLoader.Load(personId, day);

			if (adherenceDay.Changes().Any())
			{
				var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWork != null);
				var lateForWorkText = lateForWork != null ? lateForWork.LateForWork : "0";
				var minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""));
				var shiftStartTime = adherenceDay.Period().StartDateTime.AddHours(1);

				var dayInAgentTimeZone = TimeZoneInfo.ConvertTimeFromUtc(shiftStartTime, timeZone).ToDateOnly();

				_readModels.Upsert(new HistoricalOverviewReadModel
				{
					PersonId = personId,
					Date = dayInAgentTimeZone,
					WasLateForWork = lateForWork != null,
					MinutesLateForWork = minutesLateForWork,
					SecondsInAdherence = adherenceDay.SecondsInAherence(),
					SecondsOutOfAdherence = adherenceDay.SecondsOutOfAdherence(),
				});
			}
		}
	}
}