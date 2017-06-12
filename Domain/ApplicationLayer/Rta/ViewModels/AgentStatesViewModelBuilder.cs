using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		public string Name { get; set; }

		public string TeamId { get; set; }
		public string TeamName { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }

		public Guid? StateId { get; set; }
		public string State { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public string NextActivityStartTime { get; set; }
		public string Rule { get; set; }
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

	public class AgentStateFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; }// include
		public IEnumerable<Guid> TeamIds { get; set; }// include

		public IEnumerable<Guid> SkillIds { get; set; } // filter
		public IEnumerable<Guid?> ExcludedStates { get; set; } // filter
		public bool InAlarm { get; set; } // filter
	}

	public class AgentStatesViewModelBuilder
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IUserCulture _culture;
		private readonly ProperAlarm _appliedAlarm;
		private readonly IAgentStateReadModelReader _reader;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;

		public AgentStatesViewModelBuilder(
			INow now,
			IUserTimeZone timeZone,
			IUserCulture culture,
			ProperAlarm appliedAlarm,
			IAgentStateReadModelReader reader,
			ICommonAgentNameProvider nameDisplaySetting
			)
		{
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
			_appliedAlarm = appliedAlarm;
			_reader = reader;
			_nameDisplaySetting = nameDisplaySetting;
		}
		
		public AgentStatesViewModel For(AgentStateFilter filter)
		{
			return build(_reader.Read(filter));
		}
		
		public AgentStatesViewModel ForSites(Guid[] siteIds)
		{
			return build(_reader.Read(new AgentStateFilter {SiteIds = siteIds}));
		}

		public AgentStatesViewModel ForTeams(Guid[] teamIds)
		{
			return build(_reader.Read(new AgentStateFilter { TeamIds = teamIds }));
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
			var nameDisplayedAs = _nameDisplaySetting.CommonAgentNameSettings;
			return from state in states
				   where !state.IsDeleted
				   let timeInAlarm = calculateTimeInAlarm(state)
				   select new AgentStateViewModel
				   {
					   PersonId = state.PersonId,
					   Name = nameDisplayedAs.BuildCommonNameDescription(state.FirstName, state.LastName, state.EmploymentNumber),
					   SiteId = state.SiteId.ToString(),
					   SiteName = state.SiteName,
					   TeamId = state.TeamId.ToString(),
					   TeamName = state.TeamName,
					   State = state.StateName,
					   StateId = state.StateGroupId,
					   Activity = state.Activity,
					   NextActivity = state.NextActivity,
					   NextActivityStartTime = formatTime(state.NextActivityStartTime),
					   Rule = state.RuleName,
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