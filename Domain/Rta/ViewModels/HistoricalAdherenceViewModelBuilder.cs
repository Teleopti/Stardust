using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta.ViewModels
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
			var person = _persons.Load(personId);

			var adherenceDay = _agentAdherenceDayLoader
				.Load(
					personId,
					date
				);

			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(_now.UtcDateTime()),
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = buildSchedules(_schedule.Load(personId, date)),
				Changes = buildChanges(adherenceDay.Changes()),
				OutOfAdherences = buildOutOfAdherences(adherenceDay.OutOfAdherences()),
				RecordedOutOfAdherences = buildOutOfAdherences(adherenceDay.RecordedOutOfAdherences()),
				ApprovedPeriods = buildApprovedPeriods(adherenceDay.ApprovedPeriods()),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(adherenceDay.Period().StartDateTime),
					EndTime = formatForUser(adherenceDay.Period().EndDateTime)
				},
				AdherencePercentage = adherenceDay.Percentage(),
				Navigation = buildNavigation(person)
			};
		}

		private HistoricalAdherenceNavigationViewModel buildNavigation(IPerson person)
		{
			var timeZone = person?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			var agentNow = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone);
			return new HistoricalAdherenceNavigationViewModel
			{
				First = agentNow.AddDays(_displayPastDays * -1).ToString("yyyyMMdd"),
				Last = agentNow.ToString("yyyyMMdd")
			};
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

		private IEnumerable<OutOfAdherenceViewModel> buildOutOfAdherences(IEnumerable<OutOfAdherencePeriod> periods)
		{
			return periods
				.Select(x =>
					new OutOfAdherenceViewModel
					{
						StartTime = formatForUser(x.StartTime),
						EndTime = formatForUser(x.EndTime)
					}
				)
				.ToArray();
		}

		private IEnumerable<ApprovedPeriodViewModel> buildApprovedPeriods(IEnumerable<ApprovedPeriod> periods)
		{
			return periods
				.Select(a => new ApprovedPeriodViewModel
				{
					StartTime = formatForUser(a.StartTime),
					EndTime = formatForUser(a.EndTime)
				})
				.ToArray();
		}

		private IEnumerable<HistoricalChangeViewModel> buildChanges(IEnumerable<HistoricalChange> changes)
		{
			return changes
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