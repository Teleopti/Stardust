using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMultiplicatorDefinitionSetRepository : IMultiplicatorDefinitionSetRepository
	{
		private readonly IList<IMultiplicatorDefinitionSet> storage = new List<IMultiplicatorDefinitionSet>();

		public void Add(IMultiplicatorDefinitionSet root)
		{
			storage.Add(root);
		}

		public void Remove(IMultiplicatorDefinitionSet root)
		{
			throw new NotImplementedException();
		}

		public IMultiplicatorDefinitionSet Get(Guid id)
		{
			return storage.FirstOrDefault(m => id == m.Id);
		}

		public IList<IMultiplicatorDefinitionSet> LoadAll()
		{
			return storage;
		}

		public IMultiplicatorDefinitionSet Load(Guid id)
		{
			return Get(id);
		}

		public void AddRange(IEnumerable<IMultiplicatorDefinitionSet> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IMultiplicatorDefinitionSet> FindAllOvertimeDefinitions()
		{
			throw new NotImplementedException();
		}

		public IList<IMultiplicatorDefinitionSet> FindAllShiftAllowanceDefinitions()
		{
			throw new NotImplementedException();
		}

		public IList<IMultiplicatorDefinitionSet> FindAllDefinitions()
		{
			throw new NotImplementedException();
		}
	}
}