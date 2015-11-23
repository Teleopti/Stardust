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
		IEnumerable<AgentStateViewModel> ForSites(Guid[] siteIds);
		IEnumerable<AgentStateViewModel> ForTeams(Guid[] teamIds);
	}

	public class GetAgentStates : IGetAgentStates
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public GetAgentStates(IAgentStateReadModelReader agentStateReadModelReader, INow now, IUserTimeZone timeZone)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_now = now;
			_timeZone = timeZone;
		}

		public IEnumerable<AgentStateViewModel> ForSites(Guid[] siteIds)
		{
			return map(_agentStateReadModelReader.LoadForSites(siteIds));
		}

		public IEnumerable<AgentStateViewModel> ForTeams(Guid[] teamIds)
		{
			return map(_agentStateReadModelReader.LoadForTeams(teamIds));
		}

		private IEnumerable<AgentStateViewModel> map(IEnumerable<AgentStateReadModel> states)
		{
			return states.Select(x => new AgentStateViewModel
			{
				PersonId = x.PersonId,
				State = x.State,
				StateStart = x.StateStart,
				Activity = x.Scheduled,
				NextActivity = x.ScheduledNext,
				NextActivityStartTime = toUserTimeZone(x.NextStart),
				Alarm = x.AlarmName,
				AlarmStart = x.AlarmStart,
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(x.Color ?? Color.White.ToArgb())),
				TimeInState = x.StateStart.HasValue ? (int)(_now.UtcDateTime() - x.StateStart.Value).TotalSeconds : 0
			});
		}

		private DateTime? toUserTimeZone(DateTime? timestamp)
		{
			if (timestamp.HasValue)
				return TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			return null;
		}
	}
}