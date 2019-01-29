using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeTeamRepository : ITeamRepository, IEnumerable<ITeam>
	{
		private readonly FakeBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IList<ITeam> _teams = new List<ITeam>();

		public FakeTeamRepository(FakeBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}
		
		public void Add (ITeam entity)
		{
			_teams.Add (entity);
		}

	    public void Has(ITeam team)
	    {
	        _teams.Add(team);
	    }

		public void HasConnectedToCurrentBusinessUnit(ITeam team)
		{
			team.Site = new Site("_");
			var bu = _currentBusinessUnit.Current();
			bu.AddSite(team.Site);
			_businessUnitRepository.Has(bu);
		}

		public void Remove (ITeam entity)
		{
			throw new NotImplementedException();
		}

		public ITeam Get (Guid id)
		{
			return _teams.FirstOrDefault(t => t.Id.GetValueOrDefault() == id);
		}

		public IEnumerable<ITeam> LoadAll()
		{
			return _teams;
		}

		public ITeam Load (Guid id)
		{
			return _teams.First(x => x.Id.Equals(id));
		}

		public ICollection<ITeam> FindAllTeamByDescription()
		{
			return _teams;
		}

		public ICollection<ITeam> FindTeamByDescriptionName (string name)
		{
			throw new NotImplementedException();
		}

		public ICollection<ITeam> FindTeams (IEnumerable<Guid> teamId)
		{
			return _teams.Where(team => teamId.Contains(team.Id.Value) && team.IsChoosable).ToList();
		}

		public IEnumerable<ITeam> FindTeamsContain(string searchString, int maxHits)
		{
			return _teams.Where(x => x.Description.Name.Contains(searchString)).Take(maxHits);
		}

		public IEnumerable<ITeam> FindTeamsForSite(Guid siteId)
		{
			return _teams.Where(t => t.Site.Id.Equals(siteId))
				.OrderBy(t => t.Description.Name)
				.ToList();
		}

		public IEnumerator<ITeam> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}