using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeTeamRepository : ITeamRepository, IEnumerable<ITeam>
	{

		private readonly IList<ITeam> _teams = new List<ITeam>();


		public FakeTeamRepository(ITeam team)
		{
			Has(team);
		}

		public void Has(ITeam team)
		{
			_teams.Add(team);
		}

		public void Add (ITeam entity)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public ITeam Load (Guid id)
		{
			throw new NotImplementedException();
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
			return _teams;
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