using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetAgentStates
	{
		IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds);
		IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds);
	}

	public class GetAgentStates : IGetAgentStates
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IUserCulture _culture;

		public GetAgentStates(IAgentStateReadModelReader agentStateReadModelReader, INow now, IUserTimeZone timeZone, IUserCulture culture)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
		}

		public IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds)
		{
			return map(_agentStateReadModelReader.LoadForSites(siteIds));
		}

		public IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds)
		{
			return map(_agentStateReadModelReader.LoadForTeams(teamIds));
		}

		private IEnumerable<AgentStatusViewModel> map(IEnumerable<AgentStateReadModel> states)
		{
			return states.Select(x => new AgentStatusViewModel
			{
				PersonId = x.PersonId,
				State = x.State,
				StateStartTime = x.StateStartTime,
				Activity = x.Scheduled,
				NextActivity = x.ScheduledNext,
				NextActivityStartTime = formatTime(x.NextStart),
				Alarm = x.AlarmName,
				AdherenceStartTime = x.AdherenceStartTime,
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(x.Color ?? Color.White.ToArgb())),
				TimeInState = x.StateStartTime.HasValue ? (int)(_now.UtcDateTime() - x.StateStartTime.Value).TotalSeconds : 0
			});
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