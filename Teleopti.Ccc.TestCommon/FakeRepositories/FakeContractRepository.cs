using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeContractRepository : IContractRepository
	{
		private readonly IList<IContract> _contracts = new List<IContract>();

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
			return _contracts.FirstOrDefault(x => id == x.Id);
		}

		public IList<IContract> LoadAll()
		{
			return _contracts.ToArray();
		}

		public IContract Load(Guid id)
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