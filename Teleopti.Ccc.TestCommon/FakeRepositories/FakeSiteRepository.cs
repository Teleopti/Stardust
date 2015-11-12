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
		private List<ISite> _data = new List<ISite>(); 

		public void Add(ISite root)
		{
			_data.Add(root);
		}

		public void Remove(ISite root)
		{
			throw new NotImplementedException();
		}

		public void Has(ISite site)
		{
			_data.Add(site);
		}

		public ISite Get(Guid id)
		{
			return _data.Single(x => x.Id == id);
		}

		public IList<ISite> LoadAll()
		{
			return _data;
		}

		public ISite Load(Guid id)
		{
			return _data.First(x => x.Id.Equals(id));
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

		public IEnumerable<ISite> FindSitesStartWith(string searchString)
		{
			return _data.Where(x => x.Description.Name.StartsWith(searchString));
		}
	}
}