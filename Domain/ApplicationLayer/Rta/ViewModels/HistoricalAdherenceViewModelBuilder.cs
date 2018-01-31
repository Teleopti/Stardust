﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentAdherenceDay
	{
		private readonly Guid _personId;
		private readonly DateOnly _date;
		private readonly DateTime _startTime;
		private readonly DateTime _endTime;

		public AgentAdherenceDay(
			Guid personId,
			DateOnly date,
			DateTime startTime,
			DateTime endTime
		)
		{
			_personId = personId;
			_date = date;
			_startTime = startTime;
			_endTime = endTime;
		}

		public IEnumerable<HistoricalChange> Changes(IHistoricalChangeReadModelReader changes)
		{
			return changes.Read(_personId, _startTime, _endTime)
				.GroupBy(y => new
				{
					y.Timestamp,
					y.ActivityName,
					y.ActivityColor,
					y.StateName,
					y.RuleColor,
					y.RuleName,
					y.Adherence
				})
				.Select(x => x.First())
				.ToArray();
		}
	}

	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IHistoricalAdherenceReadModelReader _adherences;
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _persons;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IHistoricalChangeReadModelReader _changes;
		private readonly AdherencePercentageCalculator _calculator;
		private readonly IApprovedPeriodsReader _approvedPeriods;
		private readonly int _displayPastDays;

		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader adherences,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now,
			IUserTimeZone timeZone,
			IHistoricalChangeReadModelReader changes,
			AdherencePercentageCalculator calculator,
			IConfigReader config,
			IApprovedPeriodsReader approvedPeriods
		)
		{
			_adherences = adherences;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
			_timeZone = timeZone;
			_changes = changes;
			_calculator = calculator;
			_approvedPeriods = approvedPeriods;
			_displayPastDays = HistoricalAdherenceMaintainer.DisplayPastDays(config);
		}

		public HistoricalAdherenceViewModel Build(Guid personId, DateOnly date)
		{
			var data = new data
			{
				Now = _now.UtcDateTime(),
				PersonId = personId,
				Person = _persons.Load(personId),
			};

			loadCommonInto(data);
			data.Date = date;
			loadScheduleInto(data);
			loadDisplayInto(data);
			data.AdherenceDay = new AgentAdherenceDay(
				data.PersonId, 
				data.Date, 
				data.DisplayStartTime, 
				data.DisplayEndTime);
			loadAdherencesInto(data);

			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(data.Now),
				PersonId = personId,
				AgentName = data.Person?.Name.ToString(),
				Schedules = buildSchedules(data.Schedule),
				Changes = buildChanges(data),
				OutOfAdherences = buildOutOfAdherences(data.Adherences),
				RecordedOutOfAdherences = buildOutOfAdherences(data.Adherences),
				ApprovedPeriods = buildApprovedPeriods(data),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(data.DisplayStartTime),
					EndTime = formatForUser(data.DisplayEndTime)
				},
				AdherencePercentage = _calculator.Calculate(data.ShiftStartTime, data.ShiftEndTime, data.Adherences),
				Navigation = new HistoricalAdherenceNavigationViewModel
				{
					First = data.AgentNow.AddDays(_displayPastDays * -1).ToString("yyyyMMdd"),
					Last = data.AgentNow.ToString("yyyyMMdd")
				}
			};
		}

		private class data
		{
			public AgentAdherenceDay AdherenceDay;

			public DateTime Now;
			public DateTime AgentNow;
			public DateOnly Date;
			public Guid PersonId;
			public IPerson Person;
			public TimeZoneInfo TimeZone;

			public DateTime DisplayStartTime;
			public DateTime DisplayEndTime;

			public IEnumerable<IVisualLayer> Schedule;
			public DateTime? ShiftStartTime;
			public DateTime? ShiftEndTime;

			public IEnumerable<HistoricalAdherence> Adherences;
		}

		private void loadCommonInto(data data)
		{
			data.TimeZone = data.Person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			data.AgentNow = TimeZoneInfo.ConvertTimeFromUtc(data.Now, data.TimeZone);
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
					.ScheduledDay(data.Date)
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

		private IEnumerable<AgentOutOfAdherenceViewModel> buildOutOfAdherences(IEnumerable<HistoricalAdherence> data)
		{
			var seed = Enumerable.Empty<AgentOutOfAdherenceViewModel>();
			return data.Aggregate(seed, (x, model) =>
				{
					if (model.Adherence == HistoricalAdherenceAdherence.Out)
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

		private IEnumerable<ApprovedPeriodViewModel> buildApprovedPeriods(data data)
		{
			return _approvedPeriods
				.Read(data.PersonId, data.DisplayStartTime, data.DisplayEndTime)
				.Select(a => new ApprovedPeriodViewModel
				{
					StartTime = formatForUser(a.StartTime),
					EndTime = formatForUser(a.EndTime)
				})
				.ToArray();
		}

		private IEnumerable<HistoricalChangeViewModel> buildChanges(data data)
		{
			return data
				.AdherenceDay
				.Changes(_changes)
				.Select(x => new HistoricalChangeViewModel
				{
					Time = formatForUser(x.Timestamp),
					Activity = x.ActivityName,
					ActivityColor = x.ActivityColor.HasValue
						? ColorTranslator.ToHtml(Color.FromArgb(x.ActivityColor.Value))
						: null,
					State = x.StateName,
					Rule = x.RuleName,
					RuleColor = x.RuleColor.HasValue ? ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor.Value)) : null,
					Adherence = nameForAdherence(x.Adherence),
					AdherenceColor = colorForAdherence(x.Adherence)
				})
				.ToArray();
		}

		private class internalHistoricalAdherenceActivityViewModel : HistoricalAdherenceActivityViewModel
		{
			public DateTime StartDateTime { get; set; }
			public DateTime EndDateTime { get; set; }
		}

		private string nameForAdherence(HistoricalChangeAdherence? adherence)
		{
			if (!adherence.HasValue)
				return null;

			switch (adherence.Value)
			{
				case HistoricalChangeAdherence.In:
					return UserTexts.Resources.InAdherence;
				case HistoricalChangeAdherence.Neutral:
					return UserTexts.Resources.NeutralAdherence;
				case HistoricalChangeAdherence.Out:
					return UserTexts.Resources.OutOfAdherence;
				default:
					throw new ArgumentOutOfRangeException(nameof(adherence), adherence, null);
			}
		}

		private static string colorForAdherence(HistoricalChangeAdherence? adherence)
		{
			if (!adherence.HasValue)
				return null;

			switch (adherence.Value)
			{
				case HistoricalChangeAdherence.In:
					return ColorTranslator.ToHtml(Color.FromArgb(Color.DarkOliveGreen.ToArgb()));
				case HistoricalChangeAdherence.Neutral:
					return ColorTranslator.ToHtml(Color.FromArgb(Color.LightSalmon.ToArgb()));
				case HistoricalChangeAdherence.Out:
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