using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain
{
	public class BelongsToDateMapper
	{
		private readonly IPersonRepository _persons;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IScenarioRepository _scenarios;
		private readonly IScheduleStorage _scheduleStorage;

		public BelongsToDateMapper(
			IPersonRepository persons,
			IBusinessUnitRepository businessUnits,
			IScenarioRepository scenarios,
			IScheduleStorage scheduleStorage
		)
		{
			_persons = persons;
			_businessUnits = businessUnits;
			_scenarios = scenarios;
			_scheduleStorage = scheduleStorage;
		}

		public DateOnly? BelongsToDate(Guid personId, DateTime startDateTime, DateTime endDateTime)
		{
			var person = _persons.Get(personId);
			if (person == null)
				return new DateOnly(startDateTime);

			var period = new DateTimePeriod(startDateTime, endDateTime);
			var periods = loadPeriodsForPerson(period, person);
			return belongsToDate(periods, period, () => person.PermissionInformation.DefaultTimeZone());
		}

		public DateOnly? BelongsToDate(IEnumerable<ScheduledActivity> schedule, DateTime time, Func<TimeZoneInfo> timeZone)
		{
			var periods = schedule
				.Select(x => new dateForPeriod {Date = x.BelongsToDate, Period = new DateTimePeriod(x.StartDateTime, x.EndDateTime)})
				.ToArray();
			var period = new DateTimePeriod(time, time);
			return belongsToDate(periods, period, timeZone);
		}

		private class dateForPeriod
		{
			public DateOnly Date;
			public DateTimePeriod Period;
		}

		private DateOnly belongsToDate(IEnumerable<dateForPeriod> periods, DateTimePeriod period, Func<TimeZoneInfo> timeZone)
		{
			var date = firstIntersectingPeriod(periods, period) ??
					   startingPeriod(periods, period) ??
					   endedPeriod(periods, period);
			if (date.HasValue)
				return date.Value;
			var agentsTime = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, timeZone.Invoke());
			return new DateOnly(agentsTime);
		}

		private static DateOnly? firstIntersectingPeriod(IEnumerable<dateForPeriod> periods, DateTimePeriod period)
		{
			return periods.FirstOrDefault(l => l.Period.Intersect(period))?.Date;
		}

		private DateOnly? endedPeriod(IEnumerable<dateForPeriod> periods, DateTimePeriod period)
		{
			return (
					from p in periods
					let extendedEndTime = new DateTimePeriod(p.Period.StartDateTime, p.Period.EndDateTime.AddHours(1))
					let ended = extendedEndTime.Intersect(period)
					where ended
					select p
				)
				.FirstOrDefault()?.Date;
		}

		private DateOnly? startingPeriod(IEnumerable<dateForPeriod> periods, DateTimePeriod period)
		{
			return (
					from p in periods
					let extendedStartTime = new DateTimePeriod(p.Period.StartDateTime.AddHours(-1), p.Period.EndDateTime)
					let starting = extendedStartTime.Intersect(period)
					where starting
					select p
				)
				.FirstOrDefault()?.Date;
		}

		private IEnumerable<dateForPeriod> loadPeriodsForPerson(DateTimePeriod period, IPerson person)
		{
			var from = period.StartDateTime.ToDateOnly().AddDays(-1);
			var to = period.EndDateTime.ToDateOnly().AddDays(1);
			var days = new DateOnlyPeriod(from, to);

			var businessUnitId = person.Period(from)?.Team?.Site?.BusinessUnit?.Id;
			if (businessUnitId == null)
				return Enumerable.Empty<dateForPeriod>();
			var businessUnit = _businessUnits.Load(businessUnitId.GetValueOrDefault());
			var scenario = _scenarios.LoadDefaultScenario(businessUnit);
			if (scenario == null)
				return Enumerable.Empty<dateForPeriod>();

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				days,
				scenario);

			return (
					from scheduleDay in schedules[person].ScheduledDayCollection(days)
					let belongsToDate = scheduleDay.DateOnlyAsPeriod.DateOnly
					where days.Contains(belongsToDate)
					from layer in scheduleDay.ProjectionService().CreateProjection()
					select new dateForPeriod
					{
						Date = new DateOnly(belongsToDate.Date),
						Period = new DateTimePeriod(layer.Period.StartDateTime, layer.Period.EndDateTime)
					})
				.ToArray();
		}
	}
}