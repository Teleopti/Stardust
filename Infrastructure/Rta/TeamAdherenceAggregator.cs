using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class TeamAdherenceAggregator : ITeamAdherenceAggregator
	{
		private readonly IAgentStateReadModelReader _statisticRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public TeamAdherenceAggregator(IAgentStateReadModelReader statisticRepository, ITeamRepository teamRepository, IPersonRepository personRepository, INow now)
		{
			_statisticRepository = statisticRepository;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_now = now;
		}

		public int Aggregate(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var personIds = new List<Guid>();
			var today = _now.LocalDateOnly();
			var timePeriod = new DateOnlyPeriod(today, today);
			personIds.AddRange(_personRepository.FindPeopleBelongTeam(team, timePeriod).Select(x => x.Id.GetValueOrDefault()));
			var lastStates = _statisticRepository.Load(personIds);
			return lastStates.Count(x => AdherenceInfo.AggregatorAdherence(x) == EventAdherence.Out);
		}
	}
}