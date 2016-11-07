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
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;

		public AgentStatesViewModelBuilder(
			INow now,
			IUserTimeZone timeZone,
			IUserCulture culture,
			ProperAlarm appliedAlarm,
			IAgentStateReadModelReader agentStateReadModelReader
			)
		{
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
			_appliedAlarm = appliedAlarm;
			_agentStateReadModelReader = agentStateReadModelReader;
		}
		
		public AgentStatesViewModel For(IEnumerable<Guid> sites, IEnumerable<Guid> teams, IEnumerable<Guid> skills)
		{
			if (sites != null && skills != null)
				return build(_agentStateReadModelReader.LoadForSitesAndSkills(sites, skills));
			if (sites != null)
				return build(_agentStateReadModelReader.LoadForSites(sites));
			if (teams != null && skills != null)
				return build(_agentStateReadModelReader.LoadForTeamsAndSkills(teams, skills));
			if (teams != null)
				return build(_agentStateReadModelReader.LoadForTeams(teams));
			return build(_agentStateReadModelReader.LoadForSkills(skills));
		}
		
		public AgentStatesViewModel InAlarmFor(IEnumerable<Guid> sites, IEnumerable<Guid> teams, IEnumerable<Guid> skills)
		{
			if (sites != null && skills != null)
				return build(_agentStateReadModelReader.LoadAlarmsForSitesAndSkills(sites, skills));
			if(sites != null)
				return build(_agentStateReadModelReader.LoadAlarmsForSites(sites));
			if (teams != null && skills != null)
				return build(_agentStateReadModelReader.LoadAlarmsForTeamsAndSkills(teams, skills));
			if (teams != null)
				return build(_agentStateReadModelReader.LoadAlarmsForTeams(teams));
			return build(_agentStateReadModelReader.LoadAlarmsForSkills(skills));
		}
		
		public AgentStatesViewModel InAlarmExcludingPhoneStatesFor(ViewModelFilter filter, IEnumerable<Guid?> excludedPhoneStates)
		{
			if (filter.SiteIds != null && filter.SkillIds != null)
				return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForSitesAndSkill(filter.SiteIds, filter.SkillIds, excludedPhoneStates));
			if (filter.SiteIds != null)
				return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForSites(filter.SiteIds, excludedPhoneStates));
			if (filter.TeamIds != null && filter.SkillIds != null)
				return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(filter.TeamIds, filter.SkillIds, excludedPhoneStates));
			if (filter.TeamIds != null)
				return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForTeams(filter.TeamIds, excludedPhoneStates));
			return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForSkills(filter.SkillIds, excludedPhoneStates));
		}


		public AgentStatesViewModel ForSites(Guid[] siteIds)
		{
			return build(_agentStateReadModelReader.LoadForSites(siteIds));
		}

		public AgentStatesViewModel InAlarmForSites(Guid[] siteIds)
		{
			return build(_agentStateReadModelReader.LoadAlarmsForSites(siteIds));
		}

		public AgentStatesViewModel InAlarmForSites(Guid[] siteIds, Guid?[] excludedStateGroupIds)
		{
			return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForSites(siteIds, excludedStateGroupIds));
		}

		public AgentStatesViewModel ForTeams(Guid[] teamIds)
		{
			return build(_agentStateReadModelReader.LoadForTeams(teamIds));
		}

		public AgentStatesViewModel InAlarmForTeams(Guid[] teamIds)
		{
			return build(_agentStateReadModelReader.LoadAlarmsForTeams(teamIds));
		}

		public AgentStatesViewModel InAlarmForTeams(Guid[] teamIds, Guid?[] excludedStateGroupIds)
		{
			return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForTeams(teamIds, excludedStateGroupIds));
		}

		public AgentStatesViewModel ForSkills(Guid[] skills)
		{
			return build(_agentStateReadModelReader.LoadForSkills(skills));
		}

		public AgentStatesViewModel InAlarmForSkills(Guid[] skills)
		{
			return build(_agentStateReadModelReader.LoadAlarmsForSkills(skills));
		}

		public AgentStatesViewModel InAlarmForSkills(Guid[] skills, Guid?[] excludedStateGroupIds)
		{
			return build(_agentStateReadModelReader.LoadInAlarmExcludingPhoneStatesForSkills(skills, excludedStateGroupIds));
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
			return states
				.Where(x => !x.IsDeleted)
				.Select(x =>
				{
					var timeInAlarm = calculateTimeInAlarm(x);
					return new AgentStateViewModel
					{
						PersonId = x.PersonId,
						State = x.StateName,
						StateId = x.StateGroupId,
						Activity = x.Activity,
						NextActivity = x.NextActivity,
						NextActivityStartTime = formatTime(x.NextActivityStartTime),
						Alarm = x.RuleName,
						Color = _appliedAlarm.ColorTransition(x, timeInAlarm),
						TimeInState = x.StateStartTime.HasValue ? (int) (_now.UtcDateTime() - x.StateStartTime.Value).TotalSeconds : 0,
						TimeInAlarm = timeInAlarm,
						TimeInRule = x.RuleStartTime.HasValue ? (int?) (_now.UtcDateTime() - x.RuleStartTime.Value).TotalSeconds : null,
						Shift = x.Shift?.Select(y => new AgentStateActivityViewModel
						{
							Color = ColorTranslator.ToHtml(Color.FromArgb(y.Color)),
							StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
							EndTime = TimeZoneHelper.ConvertFromUtc(y.EndTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
							Name = y.Name
						}),
						OutOfAdherences = x.OutOfAdherences?.Select(y =>
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
				});
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