using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly ITeamRepository _teamRepository;

		public FakeNumberOfAgentsInTeamReader(ITeamRepository teamRepository)
		{
			_teamRepository = teamRepository;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(ITeam[] teams)
		{
			// simple stub implementation for now
			return _teamRepository.LoadAll()
				.ToDictionary(s => s.Id.GetValueOrDefault(), s => 0);
		}
	}
}