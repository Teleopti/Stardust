using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentStatesViewModel
	{
		public DateTime Time { get; set; }
		public IEnumerable<AgentStateViewModel> States { get; set; }
	}

	public class AgentStateViewModel
	{
		public Guid PersonId { get; set; }
		public string State { get; set; }
		public Guid? StateId { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public string NextActivityStartTime { get; set; }
		public string Alarm { get; set; }
		public string Color { get; set; }
		public int TimeInState { get; set; }
		public int? TimeInRule { get; set; }
		public int? TimeInAlarm { get; set; }
		public IEnumerable<AgentStateActivityViewModel> Shift { get; set; }
		public IEnumerable<AgentOutOfAdherenceViewModel> OutOfAdherences { get; set; }
	}

	public class AgentStateActivityViewModel
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Name { get; set; }
	}

	public class AgentOutOfAdherenceViewModel
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}

	public class ViewModelFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; }
		public IEnumerable<Guid> TeamIds { get; set; }
		public IEnumerable<Guid> SkillIds { get; set; }
	}

	public class AgentStatesViewModelBuilder
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IUserCulture _culture;
		private readonly ProperAlarm _appliedAlarm;
		private readonly IAgentStateReadModelLegacyReader _legacyReader;
		private readonly IAgentStateReadModelReader _reader;

		public AgentStatesViewModelBuilder(
			INow now,
			IUserTimeZone timeZone,
			IUserCulture culture,
			ProperAlarm appliedAlarm,
			IAgentStateReadModelLegacyReader legacyReader,
			IAgentStateReadModelReader reader
			)
		{
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
			_appliedAlarm = appliedAlarm;
			_legacyReader = legacyReader;
			_reader = reader;
		}
		
		public AgentStatesViewModel For(ViewModelFilter filter)
		{
			return build(_reader.ReadFor(filter.SiteIds, filter.TeamIds, filter.SkillIds));
		}
		
		public AgentStatesViewModel InAlarmFor(ViewModelFilter filter)
		{
			return build(_reader.ReadInAlarmFor(filter.SiteIds, filter.TeamIds, filter.SkillIds));
		}
		
		public AgentStatesViewModel InAlarmExcludingPhoneStatesFor(ViewModelFilter filter, IEnumerable<Guid?> excludedPhoneStates)
		{
			return build(_reader.ReadInAlarmExcludingStatesFor(filter.SiteIds, filter.TeamIds, filter.SkillIds, excludedPhoneStates));
		}


		public AgentStatesViewModel ForSites(Guid[] siteIds)
		{
			return build(_legacyReader.ReadForSites(siteIds));
		}

		public AgentStatesViewModel ForTeams(Guid[] teamIds)
		{
			return build(_legacyReader.ReadForTeams(teamIds));
		}
		
		private AgentStatesViewModel build(IEnumerable<AgentStateReadModel> states)
		{
			return new AgentStatesViewModel
			{
				Time = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()),
				States = buildStates(states)
			};
		}

		private IEnumerable<AgentStateViewModel> buildStates(IEnumerable<AgentStateReadModel> states)
		{
			return from state in states
				   where !state.IsDeleted
				   let timeInAlarm = calculateTimeInAlarm(state)
				   select new AgentStateViewModel
				   {
					   PersonId = state.PersonId,
					   State = state.StateName,
					   StateId = state.StateGroupId,
					   Activity = state.Activity,
					   NextActivity = state.NextActivity,
					   NextActivityStartTime = formatTime(state.NextActivityStartTime),
					   Alarm = state.RuleName,
					   Color = _appliedAlarm.ColorTransition(state, timeInAlarm),
					   TimeInState = state.StateStartTime.HasValue ? (int)(_now.UtcDateTime() - state.StateStartTime.Value).TotalSeconds : 0,
					   TimeInAlarm = timeInAlarm,
					   TimeInRule = state.RuleStartTime.HasValue ? (int?)(_now.UtcDateTime() - state.RuleStartTime.Value).TotalSeconds : null,
					   Shift = state.Shift?.Select(y => new AgentStateActivityViewModel
					   {
						   Color = ColorTranslator.ToHtml(Color.FromArgb(y.Color)),
						   StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
						   EndTime = TimeZoneHelper.ConvertFromUtc(y.EndTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
						   Name = y.Name
					   }),
					   OutOfAdherences = state.OutOfAdherences?.Select(y =>
					   {
						   string endTime = null;
						   if (y.EndTime.HasValue)
							   endTime = TimeZoneHelper.ConvertFromUtc(y.EndTime.Value, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss");
						   return new AgentOutOfAdherenceViewModel
						   {
							   StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
							   EndTime = endTime
						   };
					   })
				   };
		}

		private int? calculateTimeInAlarm(AgentStateReadModel x)
		{
			if (x.AlarmStartTime.HasValue)
			{
				return _now.UtcDateTime() >= x.AlarmStartTime.Value ? (int?)(_now.UtcDateTime() - x.AlarmStartTime.Value).TotalSeconds : null;
			}
			return null;
		}

		private string formatTime(DateTime? timestamp)
		{
			if (!timestamp.HasValue) return null;
			var userTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			var today = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;

			return userTime < today.AddDays(1)
				? userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
				: userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortDatePattern) + " " +
				  userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
				;
		}

	}
}