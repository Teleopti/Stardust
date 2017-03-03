using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IHistoricalAdherenceReadModelReader _reader;
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _persons;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		
		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader reader,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now,
			IUserTimeZone timeZone)
		{
			_reader = reader;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
			_timeZone = timeZone;
		}
		
		private IEnumerable<HistoricalAdherenceActivityViewModel> getCurrentSchedules(IPerson person)
		{
			var scenario = _scenario.Current();
			if (scenario == null || person == null)
				return Enumerable.Empty<HistoricalAdherenceActivityViewModel>();

			var tz = person.PermissionInformation.DefaultTimeZone();
			var utcDateTime = utcDateForSchedules(person);
			var period = new DateOnlyPeriod(utcDateTime.AddDays(-2), utcDateTime.AddDays(2));

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return (
				from scheduleDay in schedules[person].ScheduledDayCollection(period)
				from layer in scheduleDay.ProjectionService().CreateProjection()
				where layer.Period.ToDateOnlyPeriod(tz).Contains(utcDateTime)
				select new HistoricalAdherenceActivityViewModel
				{
					Name = layer.DisplayDescription().Name,
					Color = ColorTranslator.ToHtml(layer.DisplayColor()),
					StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
					EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss")
				})
				.ToArray();
		}
		
		public HistoricalAdherenceViewModel Build(Guid personId)
		{
			var person = _persons.Load(personId);
			var utcDateTime = utcDateTimeForOoa(person);
			var schedule = getCurrentSchedules(person);

			var result = _reader.Read(personId, utcDateTime, utcDateTime.AddDays(1));

			var agentNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			return new HistoricalAdherenceViewModel
			{
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = schedule,
				Now = agentNow.ToString("yyyy-MM-ddTHH:mm:ss"),
				Changes = new[]
				{
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(7).AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"),
						State = "Ready",
						Rule = "Positive",
						RuleColor = ColorTranslator.ToHtml(Color.Chocolate),
						Adherence = "Neutral",
						AdherenceColor = ColorTranslator.ToHtml(Color.Aquamarine)
					},
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(7).AddMinutes(58).ToString("yyyy-MM-ddTHH:mm:ss"),
						State = "InCall",
						Rule = "Positive",
						RuleColor = ColorTranslator.ToHtml(Color.Chocolate),
						Adherence = "Neutral",
						AdherenceColor = ColorTranslator.ToHtml(Color.Aquamarine)
					},
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(9).AddMinutes(26).ToString("yyyy-MM-ddTHH:mm:ss"),
						Activity = "Phone",
						ActivityColor = ColorTranslator.ToHtml(Color.LightGreen),
						State = "InCall",
						Rule = "Phone",
						RuleColor = ColorTranslator.ToHtml(Color.DarkOliveGreen),
						Adherence = "In adherence",
						AdherenceColor = ColorTranslator.ToHtml(Color.DarkGoldenrod)
					},
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(12).AddMinutes(03).ToString("yyyy-MM-ddTHH:mm:ss"),
						Activity = "Lunch",
						ActivityColor = ColorTranslator.ToHtml(Color.Yellow),
						State = "Logged off",
						Rule = "Lunch",
						RuleColor = ColorTranslator.ToHtml(Color.DarkOrchid),
						Adherence = "In adherence",
						AdherenceColor = ColorTranslator.ToHtml(Color.DarkGoldenrod)
					},
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(13).AddMinutes(00).ToString("yyyy-MM-ddTHH:mm:ss"),
						Activity = "Phone",
						ActivityColor = ColorTranslator.ToHtml(Color.LightGreen),
						State = "Logged off",
						Rule = "WHERE IS HE 😱 😱",
						RuleColor = ColorTranslator.ToHtml(Color.Crimson),
						Adherence = "Out of adherence",
						AdherenceColor = ColorTranslator.ToHtml(Color.DeepPink)
					},
					new HistoricalAdherenceChangeViewModel
					{
						Time = agentNow.Date.AddHours(14).AddMinutes(12).ToString("yyyy-MM-ddTHH:mm:ss"),
						Activity = "Phone",
						ActivityColor = ColorTranslator.ToHtml(Color.LightGreen),
						State = "Ready",
						Rule = "Phone",
						RuleColor = ColorTranslator.ToHtml(Color.DarkOliveGreen),
						Adherence = "In adherence",
						AdherenceColor = ColorTranslator.ToHtml(Color.DarkGoldenrod)
					}
				},
				OutOfAdherences = (result?.OutOfAdherences).EmptyIfNull().Select(y =>
				{
					string endTime = null;
					if (y.EndTime.HasValue)
						endTime = TimeZoneInfo.ConvertTimeFromUtc(y.EndTime.Value, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss");
					return new AgentOutOfAdherenceViewModel
					{
						StartTime = TimeZoneInfo.ConvertTimeFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
						EndTime = endTime
					};
				})
			};
		}

		private DateOnly utcDateForSchedules(IPerson person)
		{
			if (person == null)
				return new DateOnly(_now.UtcDateTime());

			var tz = person.PermissionInformation.DefaultTimeZone();
			return new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), tz));
		}

		private DateTime utcDateTimeForOoa(IPerson person)
		{
			if (person == null)
				return _now.UtcDateTime().Date;

			var tz = person.PermissionInformation.DefaultTimeZone();
			var tzNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), tz);
			var tzDate = tzNow.Date;
			return TimeZoneInfo.ConvertTimeToUtc(tzDate, tz);
		}
	}
}