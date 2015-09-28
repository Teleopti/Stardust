using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	public class FakeSiteRepository : ISiteRepository
	{
		private readonly IList<ISite> _data = new List<ISite>();

		public void Add(ISite root)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public ISite Load(Guid id)
		{
			throw new NotImplementedException();
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