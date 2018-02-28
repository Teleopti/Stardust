﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IHistoricalChangeReadModelReader _changes;
		private readonly IHistoricalAdherenceReadModelReader _adherences;
		private readonly IApprovedPeriodsReader _approvedPeriods;
		private readonly ScheduleLoader _scheduleLoader;
		private readonly IPersonRepository _persons;

		public AgentAdherenceDayLoader(
			INow now,
			IHistoricalChangeReadModelReader changes,
			IHistoricalAdherenceReadModelReader adherences,
			IApprovedPeriodsReader approvedPeriods,
			ScheduleLoader scheduleLoader,
			IPersonRepository persons
		)
		{
			_now = now;
			_changes = changes;
			_adherences = adherences;
			_approvedPeriods = approvedPeriods;
			_scheduleLoader = scheduleLoader;
			_persons = persons;
		}

		public AgentAdherenceDay Load(
			Guid personId,
			DateOnly date
		)
		{
			var now = _now.UtcDateTime();

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

			var changes = _changes.Read(personId, period.StartDateTime, period.EndDateTime);
			var lastAdherenceChange = _changes.Read(personId, DateTime.MinValue, period.StartDateTime).LastOrDefault();
			var adherences =
				new[] {lastAdherenceChange}
					.Concat(changes)
					.Where(x => x != null)
					.ToArray();
		
			var approvedPeriods = _approvedPeriods.Read(personId, period.StartDateTime, period.EndDateTime);

			var obj = new AgentAdherenceDay();
			obj.Load(
				personId,
				now,
				period,
				shift,
				adherences,
				approvedPeriods
			);

			return obj;
		}
	}
}