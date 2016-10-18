using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly Dictionary<Guid, int> _data = new Dictionary<Guid, int>();

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			return _data;
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return null;
		}

		public void Has(ITeam team, int numberOfAgents)
		{
			_data[team.Id.Value] = numberOfAgents;
		}
	}
}