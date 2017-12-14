using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		private readonly IAdherencePercentageCalculator _calculator;

		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader adherences,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now,
			IUserTimeZone timeZone,
			IHistoricalChangeReadModelReader changes,
			IAdherencePercentageCalculator calculator)
		{
			_adherences = adherences;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
			_timeZone = timeZone;
			_changes = changes;
			_calculator = calculator;
		}

		public HistoricalAdherenceViewModel Build(Guid personId, DateOnly? date)
		{
			var data = new data
			{
				Now = _now.UtcDateTime(),
				PersonId = personId,
				Person = _persons.Load(personId),
			};

			loadCommonInto(data);

			if (date.HasValue)
				data.Date = date.Value;
			else
				loadDateTheSillyWay(data);

			loadScheduleInto(data);
			loadDisplayInto(data);
			loadAdherencesInto(data);

			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(data.Now),
				PersonId = personId,
				AgentName = data.Person?.Name.ToString(),
				Schedules = buildSchedules(data.Schedule),
				Changes = buildChanges(personId, data.DisplayStartTime, data.DisplayEndTime),
				OutOfAdherences = buildOutOfAdherences(data.Adherences),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(data.DisplayStartTime),
					EndTime = formatForUser(data.DisplayEndTime)
				},
				AdherencePercentage = _calculator.CalculatePercentage(data.ShiftStartTime, data.ShiftEndTime, data.Adherences)
			};
		}

		private class data
		{
			public DateTime Now;
			public DateTime AgnentNow;
			public DateOnly Date;
			public Guid PersonId;
			public IPerson Person;
			public TimeZoneInfo TimeZone;

			public DateTime DisplayStartTime;
			public DateTime DisplayEndTime;

			public IEnumerable<IVisualLayer> Schedule;
			public DateTime? ShiftStartTime;
			public DateTime? ShiftEndTime;

			public IEnumerable<HistoricalAdherenceReadModel> Adherences;
		}

		[RemoveMeWithToggle(Toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786)]
		private void loadDateTheSillyWay(data data)
		{
			var date = new DateOnly(data.AgnentNow);
			IScheduleDay day = null;

			var scenario = _scenario.Current();
			if (scenario != null && data.Person != null)
			{
				var period = new DateOnlyPeriod(date.AddDays(-2), date.AddDays(2));

				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
					new[] {data.Person},
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					scenario);

				var possibleShifts = (
						from scheduleDay in schedules[data.Person].ScheduledDayCollection(period)
						where scheduleDay.DateOnlyAsPeriod.DateOnly == date.AddDays(-1)
							  || scheduleDay.DateOnlyAsPeriod.DateOnly == date
						let projection = scheduleDay.ProjectionService().CreateProjection()
						select new
						{
							scheduleDay,
							projection,
						})
					.ToArray();

				var shift = possibleShifts.SingleOrDefault(s => s.projection.Any(l => l.Period.Contains(data.Now))) ??
							possibleShifts.SingleOrDefault(s => s.scheduleDay.DateOnlyAsPeriod.DateOnly == date);

				day = shift.scheduleDay;
			}

			if (day != null)
				date = day.DateOnlyAsPeriod.DateOnly;

			data.Date = date;
		}

		private void loadCommonInto(data data)
		{
			data.TimeZone = data.Person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			data.AgnentNow = TimeZoneInfo.ConvertTimeFromUtc(data.Now, data.TimeZone);
		}

		private void loadDisplayInto(data data)
		{
			var startTime = TimeZoneInfo.ConvertTimeToUtc(data.Date.Date, data.TimeZone);
			data.DisplayStartTime = startTime;
			data.DisplayEndTime = startTime.AddDays(1);

			if (data.Schedule.Any())
			{
				data.DisplayStartTime = data.ShiftStartTime.Value.AddHours(-1);
				data.DisplayEndTime = data.ShiftEndTime.Value.AddHours(1);
			}
		}

		private void loadScheduleInto(data data)
		{
			data.Schedule = Enumerable.Empty<IVisualLayer>();

			var scenario = _scenario.Current();
			if (scenario != null && data.Person != null)
			{
				var period = new DateOnlyPeriod(data.Date, data.Date);

				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
					new[] {data.Person},
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					scenario);

				data.Schedule = schedules[data.Person]
					.ScheduledDayCollection(period)
					.Single()
					.ProjectionService()
					.CreateProjection();
			}

			if (data.Schedule.Any())
			{
				data.ShiftStartTime = data.Schedule.Min(x => x.Period.StartDateTime);
				data.ShiftEndTime = data.Schedule.Max(x => x.Period.EndDateTime);
			}
		}

		private void loadAdherencesInto(data data) =>
			data.Adherences =
				new[] {_adherences.ReadLastBefore(data.PersonId, data.DisplayStartTime)}
					.Concat(_adherences.Read(data.PersonId, data.DisplayStartTime, data.DisplayEndTime))
					.Where(x => x != null);

		private IEnumerable<internalHistoricalAdherenceActivityViewModel> buildSchedules(IEnumerable<IVisualLayer> layers)
		{
			return (
					from layer in layers
					select new internalHistoricalAdherenceActivityViewModel
					{
						Name = layer.DisplayDescription().Name,
						Color = ColorTranslator.ToHtml(layer.DisplayColor()),
						StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone())
							.ToString("yyyy-MM-ddTHH:mm:ss"),
						EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone())
							.ToString("yyyy-MM-ddTHH:mm:ss"),
						StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()),
						EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone())
					})
				.ToArray();
		}

		private IEnumerable<AgentOutOfAdherenceViewModel> buildOutOfAdherences(IEnumerable<HistoricalAdherenceReadModel> data)
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

		private IEnumerable<HistoricalChangeViewModel> buildChanges(Guid personId, DateTime startTime, DateTime endTime)
		{
			var changes = _changes.Read(personId, startTime, endTime)
				.GroupBy(y => new {y.Timestamp, y.ActivityName, y.ActivityColor, y.StateName, y.RuleColor, y.RuleName, y.Adherence})
				.Select(x => new HistoricalChangeViewModel
				{
					Time = formatForUser(x.Key.Timestamp),
					Activity = x.Key.ActivityName,
					ActivityColor = x.Key.ActivityColor.HasValue
						? ColorTranslator.ToHtml(Color.FromArgb(x.Key.ActivityColor.Value))
						: null,
					State = x.Key.StateName,
					Rule = x.Key.RuleName,
					RuleColor = x.Key.RuleColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.Key.RuleColor.Value)) : null,
					Adherence = nameForAdherence(x.Key.Adherence),
					AdherenceColor = colorForAdherence(x.Key.Adherence)
				})
				.ToArray();
			return changes;
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

	public interface IAdherencePercentageCalculator
	{
		int? CalculatePercentage(DateTime? shiftStartTime, DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherenceReadModel> data);
	}

	public class NoAdherencePercentageCalculator : IAdherencePercentageCalculator
	{
		public int? CalculatePercentage(DateTime? shiftStartTime, DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherenceReadModel> data)
		{
			return null;
		}
	}

	public class AdherencePercentageCalculator : IAdherencePercentageCalculator
	{
		private readonly INow _now;

		public AdherencePercentageCalculator(INow now)
		{
			_now = now;
		}

		public int? CalculatePercentage(DateTime? shiftStartTime, DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherenceReadModel> data)
		{
			if (!shiftStartTime.HasValue)
				return null;

			var onGoingShift = _now.UtcDateTime() < shiftEndTime.Value;
			var calculateUntil = onGoingShift ? _now.UtcDateTime() : shiftEndTime.Value;
			var adherenceAtStart = data.LastOrDefault(x => x.Timestamp <= shiftStartTime)?.Adherence ?? HistoricalAdherenceReadModelAdherence.Neutral;

			var adherenceReadModels = data
				.Select(a => new adherenceMoment {Time = a.Timestamp, Adherence = a.Adherence})
				.Append(new adherenceMoment {Time = shiftStartTime.Value, Adherence = adherenceAtStart})
				.Append(new adherenceMoment {Time = calculateUntil})
				.Where(a =>
				{
					var isOnShift = a.Time >= shiftStartTime.Value && a.Time <= shiftEndTime.Value;
					return isOnShift;
				})
				.OrderBy(x => x.Time)
				.ToArray();

			var timeInAdherence = timeIn(HistoricalAdherenceReadModelAdherence.In, adherenceReadModels);
			var timeInNeutral = timeIn(HistoricalAdherenceReadModelAdherence.Neutral, adherenceReadModels);
			var shiftTime = shiftEndTime.Value - shiftStartTime.Value;
			var timeToAdhere = shiftTime - timeInNeutral;

			if (timeToAdhere == TimeSpan.Zero)
				return 0;
			return Convert.ToInt32((timeInAdherence.TotalSeconds / timeToAdhere.TotalSeconds) * 100);
		}

		private static TimeSpan timeIn(HistoricalAdherenceReadModelAdherence adherenceType,
			IEnumerable<adherenceMoment> data) =>
			data.Aggregate(new timeAccumulated(), (t, m) =>
			{
				if (t.AccumulateSince != null)
					t.AccumulatedTime += (m.Time - t.AccumulateSince).Value;
				t.AccumulateSince = m.Adherence == adherenceType ? m?.Time : null;
				return t;
			}).AccumulatedTime;

		private class timeAccumulated
		{
			public TimeSpan AccumulatedTime { get; set; }
			public DateTime? AccumulateSince { get; set; }
		}

		private class adherenceMoment
		{
			public HistoricalAdherenceReadModelAdherence? Adherence { get; set; }
			public DateTime Time { get; set; }
		}
	}
}