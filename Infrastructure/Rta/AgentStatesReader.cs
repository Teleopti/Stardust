using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStatesReader : IAgentStateReader
	{
		private readonly IStatisticRepository _statisticRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public AgentStatesReader(IStatisticRepository statisticRepository, ITeamRepository teamRepository, IPersonRepository personRepository, INow now)
		{
			_statisticRepository = statisticRepository;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_now = now;
		}

		public IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);

			var peopleInTeam = _personRepository.FindPeopleBelongTeam(team,
				new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly()));

			var personIds = peopleInTeam.Select(p => p.Id.GetValueOrDefault());

			var agentAdherenceStateInfo = _statisticRepository.LoadLastAgentState(personIds).Select(state => new AgentAdherenceStateInfo()
			                                                                                                 {
				                                                                                                 PersonId = state.PersonId,
				                                                                                                 State = state.State,
																												 AlarmStart = state.StateStart,
				                                                                                                 Activity = state.Scheduled,
				                                                                                                 NextActivity = state.ScheduledNext,
				                                                                                                 NextActivityStartTime = state.NextStart,
				                                                                                                 Alarm = state.AlarmName,
																												 AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(state.Color))
			                                                                                                 });

			return agentAdherenceStateInfo;
		}
	}
}