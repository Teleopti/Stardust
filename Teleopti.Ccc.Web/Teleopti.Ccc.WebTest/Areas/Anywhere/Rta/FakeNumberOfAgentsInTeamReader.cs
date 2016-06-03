using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly Dictionary<Guid, int> _data = new Dictionary<Guid, int>();

		public IDictionary<Guid, int> FetchNumberOfAgents(ITeam[] teams)
		{
			return _data;
		}

		public void Has(ITeam team, int numberOfAgents)
		{
			_data[team.Id.Value] = numberOfAgents;
		}
	}
}