using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeTeamRepository : ITeamRepository, IEnumerable<ITeam>
	{

		private readonly IList<ITeam> _teams = new List<ITeam>();


		public FakeTeamRepository(params  ITeam[] teams)
		{
			teams.ForEach (Add);
		}
		
		public void Add (ITeam entity)
		{
			_teams.Add (entity);
		}

	    public void Has(ITeam team)
	    {
	        _teams.Add(team);
	    }

		public void Remove (ITeam entity)
		{
			throw new NotImplementedException();
		}

		public ITeam Get (Guid id)
		{
			return _teams.FirstOrDefault();
		}

		public IList<ITeam> LoadAll()
		{
			return _teams;
		}

		public ITeam Load (Guid id)
		{
			return _teams.First(x => x.Id.Equals(id));
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange (IEnumerable<ITeam> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<ITeam> FindAllTeamByDescription()
		{
			throw new NotImplementedException();
		}

		public ICollection<ITeam> FindTeamByDescriptionName (string name)
		{
			throw new NotImplementedException();
		}

		public ICollection<ITeam> FindTeams (IEnumerable<Guid> teamId)
		{
			return _teams.Where (team => teamId.Contains ( team.Id.Value)).ToList();
		}

		public IEnumerable<ITeam> FindTeamsContain(string searchString, int maxHits)
		{
			return _teams.Where(x => x.Description.Name.Contains(searchString)).Take(maxHits);
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