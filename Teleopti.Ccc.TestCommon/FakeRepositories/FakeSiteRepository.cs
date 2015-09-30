using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSiteRepository : ISiteRepository
	{
		private List<ISite> _sites = new List<ISite>(); 
		public void Add(ISite root)
		{
			_sites.Add(root);
		}

		public void Remove(ISite root)
		{
			throw new NotImplementedException();
		}

		public ISite Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISite> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISite Load(Guid id)
		{
			return _sites.First(x => x.Id.Equals(id));
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ISite> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<ISite> FindSiteByDescriptionName(string name)
		{
			throw new NotImplementedException();
		}
	}
}