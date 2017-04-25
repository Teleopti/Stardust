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
			var now = _now.UtcDateTime();
			var person = _persons.Load(personId);

			var timeZone = person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			var timeZoneTime = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
			var date = new DateOnly(timeZoneTime);

			var schedule = getScheduleLayers(date, person);
			var startTime = TimeZoneInfo.ConvertTimeToUtc(timeZoneTime.Date, timeZone);
			var endTime = startTime.AddDays(1);
			if (schedule.Any())
			{
				startTime = schedule.Min(x => x.Period.StartDateTime).AddHours(-1);
				endTime = schedule.Max(x => x.Period.EndDateTime).AddHours(1);
			}
			startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
			endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
			
			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(now),
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = buildSchedules(schedule),
				Changes = buildChanges(personId, startTime, endTime),
				OutOfAdherences = buildOutOfAdherences(personId, startTime, endTime),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(startTime),
					EndTime = formatForUser(endTime)
				}
			};
		}

		private IEnumerable<AgentOutOfAdherenceViewModel> buildOutOfAdherences(Guid personId, DateTime startTime, DateTime endTime)
		{
			var data = _adherences.Read(personId, startTime, endTime);
			var first = data.FirstOrDefault();
			if (first?.Adherence != HistoricalAdherenceReadModelAdherence.Out)
			{
				data = new[] { _adherences.ReadLastBefore(personId, first?.Timestamp ?? _now.UtcDateTime()) }
					.Concat(data)
					.Where(x => x != null)
					.ToArray();
			}

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

		private IEnumerable<HistoricalChangeViewModel> buildChanges(Guid personId, DateTime startTime, DateTime endTime)
		{
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
			return changes;
		}

		private IEnumerable<IVisualLayer> getScheduleLayers(DateOnly date, IPerson person)
		{
			var scenario = _scenario.Current();
			if (scenario == null || person == null)
				return Enumerable.Empty<IVisualLayer>();

			var period = new DateOnlyPeriod(date.AddDays(-2), date.AddDays(2));

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { person },
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return (
					from scheduleDay in schedules[person].ScheduledDayCollection(period)
					where scheduleDay.DateOnlyAsPeriod.DateOnly == date
					from layer in scheduleDay.ProjectionService().CreateProjection()
					select layer)
				.ToArray();
		}

		private IEnumerable<internalHistoricalAdherenceActivityViewModel> buildSchedules(IEnumerable<IVisualLayer> layers)
		{
			return (
				from layer in layers
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


		private string formatForUser(DateTime time)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(time, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss");
		}
	}
}