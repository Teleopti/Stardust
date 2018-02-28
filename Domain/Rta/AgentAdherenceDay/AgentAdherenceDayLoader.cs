using System;
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

			var approvedPeriods = _approvedPeriods.Read(personId, period.StartDateTime, period.EndDateTime);

			HistoricalAdherence last = null;
			var lastFoo = _changes.Read(personId, DateTime.MinValue, period.StartDateTime).LastOrDefault();
			if(lastFoo != null)
			{
				var lastAdherence = HistoricalAdherenceAdherence.Neutral;
				if (lastFoo.Adherence == HistoricalChangeAdherence.In)
					lastAdherence = HistoricalAdherenceAdherence.In;
				else if (lastFoo.Adherence == HistoricalChangeAdherence.Out)
					lastAdherence = HistoricalAdherenceAdherence.Out;
				last = new HistoricalAdherence
				{
					PersonId = personId,
					Timestamp = lastFoo.Timestamp,
					Adherence = lastAdherence
				};
			}

			var adherences =
				new[] {last}
					.Concat(
						changes.Select(x =>
						{
							var adherence = HistoricalAdherenceAdherence.Neutral;
							if (x.Adherence == HistoricalChangeAdherence.In)
								adherence = HistoricalAdherenceAdherence.In;
							else if (x.Adherence == HistoricalChangeAdherence.Out)
								adherence = HistoricalAdherenceAdherence.Out;

							return new HistoricalAdherence
							{
								PersonId = x.PersonId,
								Timestamp = x.Timestamp,
								Adherence = adherence
							};
						}))
					.Where(x => x != null)
				.ToArray();


			var adherences2 = new[] {_adherences.ReadLastBefore(personId, period.StartDateTime)}
				.Concat(_adherences.Read(personId, period.StartDateTime, period.EndDateTime))
				.Where(x => x != null);


			var obj = new AgentAdherenceDay();
			obj.Load(
				personId,
				now,
				period,
				shift,
				changes,
				adherences,
				approvedPeriods
			);

			return obj;
		}
	}
}