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

		public HistoricalAdherenceViewModel Build(Guid personId)
		{
			var now = _now.UtcDateTime();
			var person = _persons.Load(personId);
			var displayInfo = getSchedule(now, person);
			var data = _adherences.Read(personId, displayInfo.DisplayStartTime.Value, displayInfo.DisplayEndTime.Value);
			var dataBeforeDisplayStartTime = _adherences.ReadLastBefore(personId, displayInfo.DisplayStartTime.Value);
			
			if(dataBeforeDisplayStartTime != null)
				data = new[]{dataBeforeDisplayStartTime}.Concat(data);
			
			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(now),
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = buildSchedules(displayInfo.Schedule),
				Changes = buildChanges(personId, displayInfo.DisplayStartTime.Value, displayInfo.DisplayEndTime.Value),
				OutOfAdherences = buildOutOfAdherences(data),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(displayInfo.DisplayStartTime.Value),
					EndTime = formatForUser(displayInfo.DisplayEndTime.Value)
				},
				AdherencePercentage = _calculator.CalculatePercentage(displayInfo.ShiftStartTime, displayInfo.ShiftEndTime, data)
			};
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
					ActivityColor = x.Key.ActivityColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.Key.ActivityColor.Value)) : null,
					State = x.Key.StateName,
					Rule = x.Key.RuleName,
					RuleColor = x.Key.RuleColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.Key.RuleColor.Value)) : null,
					Adherence = nameForAdherence(x.Key.Adherence),
					AdherenceColor = colorForAdherence(x.Key.Adherence)
				})
				.ToArray();
			return changes;
		}

		private class scheduleInfo
		{
			public IEnumerable<IVisualLayer> Schedule;
			public DateTime? ShiftStartTime;
			public DateTime? ShiftEndTime;
			public DateTime? DisplayStartTime;
			public DateTime? DisplayEndTime;
		}

		private scheduleInfo getSchedule(DateTime now, IPerson person)
		{
			var timeZone = person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			var timeZoneTime = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
			var date = new DateOnly(timeZoneTime);

			var schedule = Enumerable.Empty<IVisualLayer>();

			var scenario = _scenario.Current();
			if (scenario != null && person != null)
			{
				var period = new DateOnlyPeriod(date.AddDays(-2), date.AddDays(2));

				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
					new[] {person},
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					scenario);

				var possibleShifts = (
						from scheduleDay in schedules[person].ScheduledDayCollection(period)
						where scheduleDay.DateOnlyAsPeriod.DateOnly == date.AddDays(-1)
							  || scheduleDay.DateOnlyAsPeriod.DateOnly == date
						let projection = scheduleDay.ProjectionService().CreateProjection()
						select new
						{
							scheduleDay,
							projection,
						})
					.ToArray();

				var shift = possibleShifts.SingleOrDefault(s => s.projection.Any(l => l.Period.Contains(now))) ?? possibleShifts.SingleOrDefault(s => s.scheduleDay.DateOnlyAsPeriod.DateOnly == date);

				schedule = shift.projection;
			}

			var startTime = TimeZoneInfo.ConvertTimeToUtc(timeZoneTime.Date, timeZone);
			var result = new scheduleInfo
			{
				Schedule = schedule,
				DisplayStartTime = startTime,
				DisplayEndTime = startTime.AddDays(1)
			};
			if (result.Schedule.Any())
			{
				result.ShiftStartTime = result.Schedule.Min(x => x.Period.StartDateTime);
				result.ShiftEndTime = result.Schedule.Max(x => x.Period.EndDateTime);
				result.DisplayStartTime = result.ShiftStartTime.Value.AddHours(-1);
				result.DisplayEndTime = result.ShiftEndTime.Value.AddHours(1);
			}

			return result;
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

	public interface IAdherencePercentageCalculator
	{
		int? CalculatePercentage(DateTime? startTime, DateTime? endTime, IEnumerable<HistoricalAdherenceReadModel> data);
	}

	public class NoAdherencePercentageCalculator : IAdherencePercentageCalculator
	{
		public int? CalculatePercentage(DateTime? startTime, DateTime? endTime, IEnumerable<HistoricalAdherenceReadModel> data)
		{
			return null;
		}
	}

	public class AdherencePercentagesModel
	{
		public HistoricalAdherenceReadModelAdherence? Adherence { get; set; }
		public DateTime Timestamp { get; set; }
	}

	public class AdherencePercentageCalculator : IAdherencePercentageCalculator
	{
		private readonly INow _now;

		public AdherencePercentageCalculator(INow now)
		{
			_now = now;
		}

		public int? CalculatePercentage(DateTime? startTime, DateTime? endTime, IEnumerable<HistoricalAdherenceReadModel> data)
		{
			if (!startTime.HasValue)
				return null;

			var shiftEndTime = _now.UtcDateTime() > endTime.Value ? endTime.Value : _now.UtcDateTime();
			var shiftTime = endTime.Value - startTime.Value;
			
			var adherenceReadModels = data
				.Select(a => new AdherencePercentagesModel()
				{
					Adherence = a.Adherence,
					Timestamp = a.Timestamp < startTime.Value ? startTime.Value : a.Timestamp
				})
				.Append(new AdherencePercentagesModel() {Timestamp = shiftEndTime}).ToArray();

			var timeInAdherence = timeIn(HistoricalAdherenceReadModelAdherence.In, adherenceReadModels);
			var timeInNeutral = timeIn(HistoricalAdherenceReadModelAdherence.Neutral, adherenceReadModels);
			
			return Convert.ToInt32((timeInAdherence.TotalSeconds / (shiftTime - timeInNeutral).TotalSeconds) * 100);
		}

		private static TimeSpan timeIn(HistoricalAdherenceReadModelAdherence adherenceType, IEnumerable<AdherencePercentagesModel> data)
		{
			DateTime? adherenceStartTime = null;
			return data.Aggregate(TimeSpan.Zero, (t, m) =>
			{
				if(adherenceStartTime != null)
					t += (m.Timestamp - adherenceStartTime).Value;
				adherenceStartTime = m.Adherence == adherenceType ? m?.Timestamp : null;
				
				return t;
			});
		}
	}
}