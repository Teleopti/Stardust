using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMultiplicatorDefinitionSetRepository : IMultiplicatorDefinitionSetRepository
	{
		private IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();

		public void Add(IMultiplicatorDefinitionSet root)
		{
			multiplicatorDefinitionSets.Add(root);
		}

		public void Remove(IMultiplicatorDefinitionSet root)
		{
			throw new NotImplementedException();
		}

		public IMultiplicatorDefinitionSet Get(Guid id)
		{
			return multiplicatorDefinitionSets.FirstOrDefault(m => id == m.Id);
		}

		public IList<IMultiplicatorDefinitionSet> LoadAll()
		{
			return multiplicatorDefinitionSets;
		}

		public IMultiplicatorDefinitionSet Load(Guid id)
		{
			throw new NotImplementedException();
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