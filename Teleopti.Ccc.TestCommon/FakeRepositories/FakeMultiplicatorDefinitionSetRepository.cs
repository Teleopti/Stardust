using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMultiplicatorDefinitionSetRepository : IMultiplicatorDefinitionSetRepository
	{
		public void Add(IMultiplicatorDefinitionSet root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IMultiplicatorDefinitionSet root)
		{
			throw new NotImplementedException();
		}

		public IMultiplicatorDefinitionSet Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IMultiplicatorDefinitionSet> LoadAll()
		{
			return new List<IMultiplicatorDefinitionSet>();
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