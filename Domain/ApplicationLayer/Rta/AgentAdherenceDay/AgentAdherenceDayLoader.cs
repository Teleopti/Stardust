using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IHistoricalChangeReadModelReader _changes;
		private readonly IHistoricalAdherenceReadModelReader _adherences;
		private readonly IApprovedPeriodsReader _approvedPeriods;
		private readonly ScheduleLoader _scheduleLoader;

		public AgentAdherenceDayLoader(
			INow now,
			IHistoricalChangeReadModelReader changes,
			IHistoricalAdherenceReadModelReader adherences,
			IApprovedPeriodsReader approvedPeriods,
			ScheduleLoader scheduleLoader
		)
		{
			_now = now;
			_changes = changes;
			_adherences = adherences;
			_approvedPeriods = approvedPeriods;
			_scheduleLoader = scheduleLoader;
		}

		public AgentAdherenceDay Load(
			Guid personId,
			TimeZoneInfo agentTimeZone,
			DateOnly date
		)
		{
			var now = _now.UtcDateTime();

			var schedule = _scheduleLoader.Load(personId, date);

			var shiftStartTime = default(DateTime?);
			var shiftEndTime = default(DateTime?);

			if (schedule.Any())
			{
				shiftStartTime = schedule.Min(x => x.Period.StartDateTime);
				shiftEndTime = schedule.Max(x => x.Period.EndDateTime);
			}

			var startOfDay = TimeZoneInfo.ConvertTimeToUtc(date.Date, agentTimeZone);
			var startTime = startOfDay;
			var endTime = startOfDay.AddDays(1);

			if (schedule.Any())
			{
				startTime = shiftStartTime.Value.AddHours(-1);
				endTime = shiftEndTime.Value.AddHours(1);
			}

			var changes = _changes.Read(personId, startTime, endTime);

			var adherences = new[] {_adherences.ReadLastBefore(personId, startTime)}
				.Concat(_adherences.Read(personId, startTime, endTime))
				.Where(x => x != null);

			var approvedPeriods = _approvedPeriods.Read(personId, startTime, endTime);

			var obj = new AgentAdherenceDay();
			obj.Load(
				now,
				changes,
				adherences,
				approvedPeriods,
				shiftStartTime,
				shiftEndTime
			);

			return obj;
		}
	}
}