using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStatesReader : IAgentStateReader
	{
		private readonly IRtaRepository _statisticRepository;

		public AgentStatesReader(IRtaRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId)
		{
			var agentAdherenceStateInfo = _statisticRepository.LoadTeamAgentStates(teamId)
				.Select(state => new AgentAdherenceStateInfo
				{
					PersonId = state.PersonId,
					State = state.State,
					StateStart = state.StateStart,
					Activity = state.Scheduled,
					NextActivity = state.ScheduledNext,
					NextActivityStartTime = state.NextStart,
					Alarm = state.AlarmName,
					AlarmStart = state.AlarmStart,
					AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(state.Color ?? 0))
				});

			return agentAdherenceStateInfo;
		}
	}
}