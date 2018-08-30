﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayLoaderDurationOfEvents : IAgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IRtaEventStoreReader _eventStore;
		private readonly ScheduleLoader _scheduleLoader;
		private readonly IPersonRepository _persons;

		public AgentAdherenceDayLoaderDurationOfEvents(
			INow now,
			ScheduleLoader scheduleLoader,
			IPersonRepository persons,
			IRtaEventStoreReader eventStore)
		{
			_now = now;
			_scheduleLoader = scheduleLoader;
			_persons = persons;
			_eventStore = eventStore;
		}

		public IAgentAdherenceDay Load(Guid personId, DateOnly date) => load(personId, date, DateTime.MaxValue);

		public IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date) => load(personId, date, _now.UtcDateTime());

		private IAgentAdherenceDay load(Guid personId, DateOnly date, DateTime until)
		{
			var person = _persons.Load(personId);

			var time = TimeZoneInfo.ConvertTimeToUtc(date.Date, person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc);
			var period = new DateTimePeriod(time, time.AddDays(1));

			var schedule = _scheduleLoader.Load(personId, date);
			var shift = default(DateTimePeriod?);
			if (schedule.Any())
			{
				shift = new DateTimePeriod(schedule.Min(x => x.Period.StartDateTime), schedule.Max(x => x.Period.EndDateTime));
				period = new DateTimePeriod(shift.Value.StartDateTime.AddHours(-1), shift.Value.EndDateTime.AddHours(1));
			}

			var events =
				_eventStore.LoadLastAdherenceEventBefore(personId, period.StartDateTime)
					.AsArray()
					.Concat(_eventStore.Load(personId, period))
					.Where(x => x != null)
					.ToArray();

			var obj = new AgentAdherenceDayWithDurationOfEvents(personId, period, shift, until);
			events.ForEach(x => obj.Apply((dynamic) x));
			obj.ApplyDone();

			return obj;
		}
	}
}