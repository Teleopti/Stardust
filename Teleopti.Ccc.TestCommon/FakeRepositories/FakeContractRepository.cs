using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeContractRepository : IContractRepository
	{
		private IList<IContract> _contracts = new List<IContract>();

		public void Has(IContract root)
		{
			_contracts.Add(root);
		}

		public void Add(IContract root)
		{
			_contracts.Add(root);
		}

		public void Remove(IContract root)
		{
			throw new NotImplementedException();
		}

		public IContract Get(Guid id)
		{
			return _contracts.Single(x => x.Id.Equals(id));
		}

		public IList<IContract> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IContract Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IContract> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<IContract> FindAllContractByDescription()
		{
			return _contracts.OrderBy(x => x.Description).ToArray();
		}

		public IEnumerable<IContract> FindContractsContain(string searchString, int itemsLeftToLoad)
		{
			return _contracts.Where(x => x.Description.Name.Contains(searchString)).Take(itemsLeftToLoad);
		}
	}
}