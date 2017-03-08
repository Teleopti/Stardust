﻿using System;
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
		private readonly IHistoricalChangeReadModelReader _changeReader;

		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader reader,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now,
			IUserTimeZone timeZone,
			IHistoricalChangeReadModelReader changeReader)
		{
			_reader = reader;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
			_timeZone = timeZone;
			_changeReader = changeReader;
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

			var historicalAdherence = _reader.Read(personId, utcDateTime, utcDateTime.AddDays(1));
			var outOfAdherences = (historicalAdherence?.OutOfAdherences)
				.EmptyIfNull()
				.Select(y =>
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
				.ToArray();
			var changes = _changeReader.Read(personId, utcDateTime, utcDateTime.AddDays(1))
				.Select(x => new HistoricalChangeViewModel
				{
					Time = TimeZoneInfo.ConvertTimeFromUtc(x.Timestamp, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
					Activity = x.ActivityName,
					ActivityColor = x.ActivityColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.ActivityColor.Value)) : null,
					State = x.StateName,
					Rule = x.RuleName,
					RuleColor = x.RuleColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor.Value)) : null,
					Adherence = nameForAdherence(x.Adherence),
					AdherenceColor = colorForAdherence(x.Adherence)
				})
				.ToArray();

			var agentNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			return new HistoricalAdherenceViewModel
			{
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = schedule,
				Now = agentNow.ToString("yyyy-MM-ddTHH:mm:ss"),
				Changes = changes,
				OutOfAdherences = outOfAdherences
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

		private string nameForAdherence(HistoricalChangeInternalAdherence? adherence)
		{
			if (!adherence.HasValue)
				return null;

			switch (adherence.Value)
			{
				case HistoricalChangeInternalAdherence.In:
					return "In adherence";
				case HistoricalChangeInternalAdherence.Neutral:
					return "Neutral";
				case HistoricalChangeInternalAdherence.Out:
					return "Out of adherence";
				default:
					throw new ArgumentOutOfRangeException(nameof(adherence), adherence, null);
			}
		}

		private static string colorForAdherence(HistoricalChangeInternalAdherence? adherence)
		{
			if (!adherence.HasValue)
				return null;

			switch (adherence.Value)
			{
				case HistoricalChangeInternalAdherence.In:
					return ColorTranslator.ToHtml(Color.FromArgb(Color.DarkOliveGreen.ToArgb()));
				case HistoricalChangeInternalAdherence.Neutral:
					return ColorTranslator.ToHtml(Color.FromArgb(Color.LightSalmon.ToArgb()));
				case HistoricalChangeInternalAdherence.Out:
					return ColorTranslator.ToHtml(Color.FromArgb(Color.Firebrick.ToArgb()));
				default:
					throw new ArgumentOutOfRangeException(nameof(adherence), adherence, null);
			}
		}
	}
}