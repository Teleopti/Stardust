using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IMultiplicatorDefinitionSet> FindAllOvertimeDefinitions()
		{
			return storage.Where(s => s.MultiplicatorType == MultiplicatorType.Overtime).ToList();
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