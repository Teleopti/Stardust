using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBudgetGroupRepository : IBudgetGroupRepository
	{
		private ICollection<IBudgetGroup> _budgetGroups = new List<IBudgetGroup>();
		public void Add(IBudgetGroup root)
		{
			_budgetGroups.Add(root);
		}

		public void Remove(IBudgetGroup root)
		{
			throw new NotImplementedException();
		}

		public IBudgetGroup Get(Guid id)
		{
			return _budgetGroups.FirstOrDefault(b => b.Id == id);
		}

		public IBudgetGroup Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetGroup> LoadAll()
		{
			return _budgetGroups.ToList();
		}
	}
}
