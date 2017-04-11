using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IHistoricalAdherenceReadModelReader _adherences;
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _persons;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IHistoricalChangeReadModelReader _changes;

		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader adherences,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now,
			IUserTimeZone timeZone,
			IHistoricalChangeReadModelReader changes)
		{
			_adherences = adherences;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
			_timeZone = timeZone;
			_changes = changes;
		}
		
		public HistoricalAdherenceViewModel Build(Guid personId)
		{
			var person = _persons.Load(personId);

			var timeZone = person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			var timeZoneTime = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone);
			var date = new DateOnly(timeZoneTime);

			var schedule = getCurrentSchedules(date, timeZone, person);
			var startTime = TimeZoneInfo.ConvertTimeToUtc(timeZoneTime.Date, timeZone);
			var endTime = startTime.AddDays(1);
			if (schedule.Any())
			{
				startTime = schedule.Min(x => x.StartDateTime).AddHours(-1);
				endTime = schedule.Max(x => x.EndDateTime).AddHours(1);
			}
			startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
			endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

			var historicalAdherence = _adherences.Read(personId, startTime, endTime);
			var first = historicalAdherence.FirstOrDefault();
			if (first?.Adherence != HistoricalAdherenceReadModelAdherence.Out)
			{
				historicalAdherence = new[] {_adherences.ReadLastBefore(personId, first?.Timestamp ?? _now.UtcDateTime())}
					.Concat(historicalAdherence)
					.Where(x => x != null)
					.ToArray();
			}
			var outOfAdherences = buildViewModel(historicalAdherence.EmptyIfNull());

			var changes = _changes.Read(personId, startTime, endTime)
				.Select(x => new HistoricalChangeViewModel
				{
					Time = formatForUser(x.Timestamp),
					Activity = x.ActivityName,
					ActivityColor = x.ActivityColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.ActivityColor.Value)) : null,
					State = x.StateName,
					Rule = x.RuleName,
					RuleColor = x.RuleColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor.Value)) : null,
					Adherence = nameForAdherence(x.Adherence),
					AdherenceColor = colorForAdherence(x.Adherence)
				})
				.ToArray();

			var userNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			return new HistoricalAdherenceViewModel
			{
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = schedule,
				Now = userNow.ToString("yyyy-MM-ddTHH:mm:ss"),
				Changes = changes,
				OutOfAdherences = outOfAdherences
			};
		}

		private IEnumerable<internalHistoricalAdherenceActivityViewModel> getCurrentSchedules(DateOnly date, TimeZoneInfo timeZone, IPerson person)
		{
			var scenario = _scenario.Current();
			if (scenario == null || person == null)
				return Enumerable.Empty<internalHistoricalAdherenceActivityViewModel>();

			var period = new DateOnlyPeriod(date.AddDays(-2), date.AddDays(2));

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { person },
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return (
				from scheduleDay in schedules[person].ScheduledDayCollection(period)
				from layer in scheduleDay.ProjectionService().CreateProjection()
				where layer.Period.ToDateOnlyPeriod(timeZone).Contains(date)
				select new internalHistoricalAdherenceActivityViewModel
				{
					Name = layer.DisplayDescription().Name,
					Color = ColorTranslator.ToHtml(layer.DisplayColor()),
					StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
					EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
					StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()),
					EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone())
				})
				.ToArray();
		}

		private class internalHistoricalAdherenceActivityViewModel : HistoricalAdherenceActivityViewModel
		{
			public DateTime StartDateTime { get; set; }
			public DateTime EndDateTime { get; set; }
		}
		
		private string nameForAdherence(HistoricalChangeInternalAdherence? adherence)
		{
			if (!adherence.HasValue)
				return null;

			switch (adherence.Value)
			{
				case HistoricalChangeInternalAdherence.In:
					return UserTexts.Resources.InAdherence;
				case HistoricalChangeInternalAdherence.Neutral:
					return UserTexts.Resources.NeutralAdherence;
				case HistoricalChangeInternalAdherence.Out:
					return UserTexts.Resources.OutOfAdherence;
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

		private IEnumerable<AgentOutOfAdherenceViewModel> buildViewModel(IEnumerable<HistoricalAdherenceReadModel> data)
		{
			var seed = Enumerable.Empty<AgentOutOfAdherenceViewModel>();
			return data.Aggregate(seed, (x, model) =>
				{
					if (model.Adherence == HistoricalAdherenceReadModelAdherence.Out)
					{
						if (x.IsEmpty(y => y.EndTime == null))
							x = x
								.Append(new AgentOutOfAdherenceViewModel
								{
									StartTime = formatForUser(model.Timestamp)
								})
								.ToArray();
					}
					else
					{
						var existing = x.FirstOrDefault(y => y.EndTime == null);
						if (existing != null)
							existing.EndTime = formatForUser(model.Timestamp);
					}

					return x;
				})
				.ToArray();
		}

		private string formatForUser(DateTime time)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(time, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss");
		}
	}
}