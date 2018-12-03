using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels
{
	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IPersonRepository _persons;
		private readonly IScheduleLoader _schedule;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IAgentAdherenceDayLoader _agentAdherenceDayLoader;
		private readonly int _displayPastDays;

		public HistoricalAdherenceViewModelBuilder(
			IPersonRepository persons,
			IScheduleLoader schedule,
			INow now,
			IUserTimeZone timeZone,
			IConfigReader config,
			IAgentAdherenceDayLoader agentAdherenceDayLoader
		)
		{
			_persons = persons;
			_schedule = schedule;
			_now = now;
			_timeZone = timeZone;
			_agentAdherenceDayLoader = agentAdherenceDayLoader;
			_displayPastDays = RtaEventStoreMaintainer.DisplayPastDays(config);
		}

		public HistoricalAdherenceViewModel Build(Guid personId, DateOnly date)
		{
			var person = _persons.Load(personId);

			var adherenceDay = _agentAdherenceDayLoader
				.LoadUntilNow(
					personId,
					date
				);

			return new HistoricalAdherenceViewModel
			{
				Now = formatForUser(_now.UtcDateTime()),
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = buildSchedules(_schedule.Load(personId, date), person),
				Changes = buildChanges(adherenceDay.Changes()),
				OutOfAdherences = buildOutOfAdherences(adherenceDay.OutOfAdherences()),
				RecordedOutOfAdherences = buildOutOfAdherences(adherenceDay.RecordedOutOfAdherences()),
				ApprovedPeriods = buildApprovedPeriods(adherenceDay.ApprovedPeriods()),
				Timeline = new ScheduleTimeline
				{
					StartTime = formatForUser(adherenceDay.DisplayPeriod().StartDateTime),
					EndTime = formatForUser(adherenceDay.DisplayPeriod().EndDateTime)
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

		private IEnumerable<HistoricalAdherenceActivityViewModel> buildSchedules(IEnumerable<IVisualLayer> layers, IPerson person)
		{
			return (
					from layer in layers
					select new HistoricalAdherenceActivityViewModel
					{
						Name = layer.Payload.ConfidentialDescription(person).Name,
						Color = ColorTranslator.ToHtml(layer.Payload.ConfidentialDisplayColor(person)),
						StartTime = formatForUser(layer.Period.StartDateTime),
						EndTime = formatForUser(layer.Period.EndDateTime),
					})
				.ToArray();
		}

		private IEnumerable<OutOfAdherenceViewModel> buildOutOfAdherences(IEnumerable<AdherencePeriod> periods)
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

		private IEnumerable<HistoricalChangeViewModel> buildChanges(IEnumerable<HistoricalChangeModel> changes)
		{
			return changes
				.Select(x => new HistoricalChangeViewModel
				{
					Time = formatForUser(x.Timestamp),
					Duration = x.Duration,
					LateForWork = x.LateForWork,
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
					return Ccc.UserTexts.Resources.InAdherence;
				case HistoricalChangeAdherence.Neutral:
					return Ccc.UserTexts.Resources.NeutralAdherence;
				case HistoricalChangeAdherence.Out:
					return Ccc.UserTexts.Resources.OutOfAdherence;
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