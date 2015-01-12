﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class SiteAdherenceAggregator : ISiteAdherenceAggregator
	{
		private readonly IRtaRepository _statisticRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public SiteAdherenceAggregator(IRtaRepository statisticRepository, ISiteRepository siteRepository, IPersonRepository personRepository, INow now)
		{
			_statisticRepository = statisticRepository;
			_siteRepository = siteRepository;
			_personRepository = personRepository;
			_now = now;
		}

		public int Aggregate(Guid siteId)
		{
			var site = _siteRepository.Get(siteId);
			var personIds = new List<Guid>();
			var today = _now.LocalDateOnly();
			var timePeriod = new DateOnlyPeriod(today, today);
			foreach (var team in site.TeamCollection)
			{
				personIds.AddRange(_personRepository.FindPeopleBelongTeam(team, timePeriod).Select(x => x.Id.GetValueOrDefault()));
			}
			var lastStates = _statisticRepository.LoadLastAgentState(personIds);
			return lastStates.Count(x => StateInfo.AdherenceFor(x) == Adherence.Out);
		}
	}
}