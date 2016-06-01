using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders
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
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public string NextActivityStartTime { get; set; }
		public string Alarm { get; set; }
		public string Color { get; set; }
		public int TimeInState { get; set; }
		public int? TimeInAlarm { get; set; }
		public IEnumerable<AgentStateActivityViewModel> Shift { get; set; }
	}

	public class AgentStateActivityViewModel
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Name { get; set; }
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

		public AgentStatesViewModel ForSites(Guid[] siteIds, bool inAlarm)
		{
			return build(_agentStateReadModelReader.LoadForSites(siteIds, inAlarm));
		}

		public AgentStatesViewModel ForTeams(Guid[] teamIds, bool inAlarm)
		{
			return build(_agentStateReadModelReader.LoadForTeams(teamIds, inAlarm));
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
			return states.Select(x =>
			{
				var timeInAlarm = calculateTimeInAlarm(x);
				return new AgentStateViewModel
				{
					PersonId = x.PersonId,
					State = x.StateName,
					Activity = x.Activity,
					NextActivity = x.NextActivity,
					NextActivityStartTime = formatTime(x.NextActivityStartTime),
					Alarm = x.RuleName,
					Color = _appliedAlarm.ColorTransition(x, timeInAlarm),
					TimeInState = x.StateStartTime.HasValue ? (int)(_now.UtcDateTime() - x.StateStartTime.Value).TotalSeconds : 0,
					TimeInAlarm = timeInAlarm,
					Shift = x.Shift?.Select(y => new AgentStateActivityViewModel
					{
						Color= ColorTranslator.ToHtml(Color.FromArgb(y.Color)),
						StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime.Utc(), _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
						EndTime = TimeZoneHelper.ConvertFromUtc(y.EndTime.Utc(), _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH:mm:ss"),
						Name = y.Name
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