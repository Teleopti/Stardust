using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public class AgentAdherenceDayLoaderCancelAdjustment : IAgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IRtaEventStoreReader _eventStore;
		private readonly PersistedTypeMapper _typeMapper;
		private readonly IJsonEventSerializer _serializer;
		private readonly ScheduleLoader _scheduleLoader;
		private readonly IPersonRepository _persons;

		public AgentAdherenceDayLoaderCancelAdjustment(
			INow now,
			ScheduleLoader scheduleLoader,
			IPersonRepository persons,
			IRtaEventStoreReader eventStore,
			PersistedTypeMapper typeIdMapper,
			IJsonEventSerializer serializer)
		{
			_now = now;
			_scheduleLoader = scheduleLoader;
			_persons = persons;
			_eventStore = eventStore;
			_typeMapper = typeIdMapper;
			_serializer = serializer;
		}

		public IAgentAdherenceDay Load(Guid personId, DateOnly date) => load(personId, date, DateTime.MaxValue.Utc());
		public IAgentAdherenceDay LoadUntilNow(Guid personId) => LoadUntilNow(personId, new DateOnly(_now.UtcDateTime().Date));
		public IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date) => load(personId, date, _now.UtcDateTime());

		private IAgentAdherenceDay load(Guid personId, DateOnly date, DateTime until)
		{
			var person = _persons.Load(personId);

			var time = TimeZoneInfo.ConvertTimeToUtc(date.Date, person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc);
			var period = new DateTimePeriod(time, time.AddDays(1));
			var eventsForPerson = _eventStore.Load(personId, date);
			var adjustedPeriodEvents = _eventStore.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period);
			var canceledAdjustmentsEvents = _eventStore.LoadOfTypeForPeriod<PeriodAdjustmentToNeutralCanceledEvent>(period);
			var events = eventsForPerson
				.Concat(adjustedPeriodEvents)
				.Concat(canceledAdjustmentsEvents);
			var saga = new AgentAdherenceDayCancelAdjustment(until, period, () => shiftFromSchedule(personId, date));

			applyAndVerboseLogErrors(events, saga);

			return saga;
		}

		private void applyAndVerboseLogErrors(IEnumerable<IEvent> events, AgentAdherenceDayCancelAdjustment saga)
		{
			try
			{
				events.ForEach(x =>
				{
					var method = saga.GetType().GetMethod("Apply", new[] {x.GetType()});
					if (method != null)
						saga.Apply((dynamic) x);
				});
				saga.ApplyDone();
			}
			catch (Exception e)
			{
				var info = eventInformationForTestBench(events);
				throw new AgentAdherenceDayLoadException($"Loading this agents adherence failed.\n{info}", e);
			}
		}

		private string eventInformationForTestBench(IEnumerable<IEvent> events)
		{
			var stringBuilder = new StringBuilder();
			events.ForEach(e =>
			{
				var persistedName = _typeMapper.NameForPersistence(e.GetType());
				var eventData = _serializer.SerializeEvent(e);
				stringBuilder.AppendLine($"{persistedName}\t{eventData}");
			});
			return stringBuilder.ToString();
		}

		private DateTimePeriod? shiftFromSchedule(Guid personId, DateOnly date)
		{
			var schedule = _scheduleLoader.Load(personId, date);
			if (schedule.Any())
				return new DateTimePeriod(schedule.Min(x => x.Period.StartDateTime), schedule.Max(x => x.Period.EndDateTime));
			return null;
		}
	}
}