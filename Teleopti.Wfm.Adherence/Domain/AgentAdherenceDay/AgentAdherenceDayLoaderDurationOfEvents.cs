using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayLoaderDurationOfEvents : IAgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IRtaEventStoreReader _eventStore;
		private readonly IScheduleLoader _scheduleLoader;
		private readonly IPersonRepository _persons;

		public AgentAdherenceDayLoaderDurationOfEvents(
			INow now,
			IScheduleLoader scheduleLoader,
			IPersonRepository persons,
			IRtaEventStoreReader eventStore)
		{
			_now = now;
			_scheduleLoader = scheduleLoader;
			_persons = persons;
			_eventStore = eventStore;
		}

		public IAgentAdherenceDay Load(Guid personId, DateOnly date) => load(personId, date, DateTime.MaxValue.Utc());

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
				_eventStore.LoadLastAdherenceEventBefore(personId, period.StartDateTime, DeadLockVictim.Yes)
					.AsArray()
					.Concat(_eventStore.Load(personId, period.StartDateTime, period.EndDateTime))
					.Where(x => x != null)
					.ToArray();

			var obj = new AgentAdherenceDayWithDurationOfEvents(personId, period, shift, until);
			events.ForEach(x =>
			{
				var method = obj.GetType().GetMethod("Apply", new[] {x.GetType()});
				if (method != null)
					obj.Apply((dynamic) x);
			});
			obj.ApplyDone();

			return obj;
		}
	}
}