using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSiteRepository : ISiteRepository
	{
		private readonly List<ISite> _data = new List<ISite>(); 

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

		public void Has(Guid siteId)
		{
			var site = new Site(RandomName.Make())
				.WithId(siteId);
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

		public ICollection<ISite> FindSiteByDescriptionName(string name)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ISite> FindSitesContain(string searchString, int maxHits)
		{
			return _data.Where(x => x.Description.Name.Contains(searchString)).Take(maxHits);
		}

		public IEnumerable<ISite> LoadAllOrderByName()
		{
			return _data.OrderBy(x => x.Description.Name);
		}
	}
}