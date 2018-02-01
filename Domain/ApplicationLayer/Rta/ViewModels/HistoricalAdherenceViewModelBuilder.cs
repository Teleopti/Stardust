using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IPersonRepository _persons;
		private readonly ScheduleLoader _schedule;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly AgentAdherenceDayLoader _agentAdherenceDayLoader;
		private readonly int _displayPastDays;

		public HistoricalAdherenceViewModelBuilder(
			IPersonRepository persons,
			ScheduleLoader schedule,
			INow now,
			IUserTimeZone timeZone,
			IConfigReader config,
			AgentAdherenceDayLoader agentAdherenceDayLoader
		)
		{
			_persons = persons;
			_schedule = schedule;
			_now = now;
			_timeZone = timeZone;
			_agentAdherenceDayLoader = agentAdherenceDayLoader;
			_displayPastDays = HistoricalAdherenceMaintainer.DisplayPastDays(config);
		}

		public HistoricalAdherenceViewModel Build(Guid personId, DateOnly date)
		{
			var data = new data
			{
				PersonId = personId,
				Person = _persons.Load(personId),
			};

			loadAgentTimeInto(data);
			data.Date = date;
			loadDefaultDisplayInto(data);
			loadScheduleInto(data);

			data.AdherenceDay = _agentAdherenceDayLoader
				.Load(
					data.PersonId,
					data.TimeZone,
					data.Date
				);

			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(_now.UtcDateTime()),
				PersonId = personId,
				AgentName = data.Person?.Name.ToString(),
				Schedules = buildSchedules(data.Schedule),
				Changes = buildChanges(data),
				OutOfAdherences = buildOutOfAdherences(data.AdherenceDay.OutOfAdherences()),
				RecordedOutOfAdherences = buildOutOfAdherences(data.AdherenceDay.OutOfAdherences()),
				ApprovedPeriods = buildApprovedPeriods(data.AdherenceDay.ApprovedPeriods()),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(data.DisplayStartTime),
					EndTime = formatForUser(data.DisplayEndTime)
				},
				AdherencePercentage = data.AdherenceDay.Percentage(),
				Navigation = new HistoricalAdherenceNavigationViewModel
				{
					First = data.AgentNow.AddDays(_displayPastDays * -1).ToString("yyyyMMdd"),
					Last = data.AgentNow.ToString("yyyyMMdd")
				}
			};
		}

		private class data
		{
			public AgentAdherenceDay.AgentAdherenceDay AdherenceDay;

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
		}

		private void loadAgentTimeInto(data data)
		{
			data.TimeZone = data.Person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			data.AgentNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), data.TimeZone);
		}

		private void loadDefaultDisplayInto(data data)
		{
			var startTime = TimeZoneInfo.ConvertTimeToUtc(data.Date.Date, data.TimeZone);
			data.DisplayStartTime = startTime;
			data.DisplayEndTime = startTime.AddDays(1);
		}

		private void loadScheduleInto(data data)
		{
			data.Schedule = _schedule.Load(data.PersonId, data.Date);
			if (!data.Schedule.Any())
				return;
			data.ShiftStartTime = data.Schedule.Min(x => x.Period.StartDateTime);
			data.ShiftEndTime = data.Schedule.Max(x => x.Period.EndDateTime);
			data.DisplayStartTime = data.ShiftStartTime.Value.AddHours(-1);
			data.DisplayEndTime = data.ShiftEndTime.Value.AddHours(1);
		}

		private IEnumerable<HistoricalAdherenceActivityViewModel> buildSchedules(IEnumerable<IVisualLayer> layers)
		{
			return (
					from layer in layers
					select new HistoricalAdherenceActivityViewModel
					{
						Name = layer.DisplayDescription().Name,
						Color = ColorTranslator.ToHtml(layer.DisplayColor()),
						StartTime = formatForUser(layer.Period.StartDateTime),
						EndTime = formatForUser(layer.Period.EndDateTime),
					})
				.ToArray();
		}

		private IEnumerable<OutOfAdherenceViewModel> buildOutOfAdherences(IEnumerable<OutOfAdherencePeriod> data)
		{
			return data
				.Select(x =>
					new OutOfAdherenceViewModel
					{
						StartTime = formatForUser(x.StartTime),
						EndTime = formatForUser(x.EndTime)
					}
				)
				.ToArray();
		}

		private IEnumerable<ApprovedPeriodViewModel> buildApprovedPeriods(IEnumerable<ApprovedPeriod> data)
		{
			return data
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
				.Changes()
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

		private string formatForUser(DateTime? time) =>
			time.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(time.Value, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss") : null;
	}
}