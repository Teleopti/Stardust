using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Infrastructure;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayLoaderSpeedUpRemoveScheduleDependency : IAgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IRtaEventStoreReader _eventStore;
		private readonly RtaEventStoreTypeIdMapper _typeIdMapper;
		private readonly IJsonEventSerializer _serializer;
		private readonly IScheduleLoader _scheduleLoader;
		private readonly IPersonRepository _persons;
		private readonly ILog log = LogManager.GetLogger(typeof(AgentAdherenceDayLoaderSpeedUpRemoveScheduleDependency));

		public AgentAdherenceDayLoaderSpeedUpRemoveScheduleDependency(
			INow now,
			IScheduleLoader scheduleLoader,
			IPersonRepository persons,
			IRtaEventStoreReader eventStore,
			RtaEventStoreTypeIdMapper typeIdMapper,
			IJsonEventSerializer serializer)
		{
			_now = now;
			_scheduleLoader = scheduleLoader;
			_persons = persons;
			_eventStore = eventStore;
			_typeIdMapper = typeIdMapper;
			_serializer = serializer;
		}

		public IAgentAdherenceDay Load(Guid personId, DateOnly date) => load(personId, date, DateTime.MaxValue.Utc());

		public IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date) => load(personId, date, _now.UtcDateTime());

		private IAgentAdherenceDay load(Guid personId, DateOnly date, DateTime until)
		{
			var person = _persons.Load(personId);

			var time = TimeZoneInfo.ConvertTimeToUtc(date.Date, person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc);
			var period = new DateTimePeriod(time, time.AddDays(1));
			var events = _eventStore.Load(personId, date);
			var saga = new AgentAdherenceDaySpeedUpRemoveScheduleDependency(until, period, () => shiftFromSchedule(personId, date));

			applyAndVerboseLogErrors(events, saga);

			return saga;
		}

		private void applyAndVerboseLogErrors(IEnumerable<IEvent> events, AgentAdherenceDaySpeedUpRemoveScheduleDependency saga)
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
				var typeId = _typeIdMapper.EventTypeId(e);
				var eventData = _serializer.SerializeEvent(e);
				stringBuilder.AppendLine($"{typeId}\t{eventData}");
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