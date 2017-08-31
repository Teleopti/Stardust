using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMultiplicatorDefinitionSetRepository : IMultiplicatorDefinitionSetRepository
	{
		private readonly IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();

		public void Has(IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			Add(multiplicatorDefinitionSet);
		}

		public void Add(IMultiplicatorDefinitionSet root)
		{
			_multiplicatorDefinitionSets.Add(root);
		}

		public void Remove(IMultiplicatorDefinitionSet root)
		{
			_multiplicatorDefinitionSets.Remove(root);
		}

		public IMultiplicatorDefinitionSet Get(Guid id)
		{
			return _multiplicatorDefinitionSets.FirstOrDefault(m => m.Id.Value == id);
		}


		public IMultiplicatorDefinitionSet Load(Guid id)
		{
			return Get(id);
		}

		public IList<IMultiplicatorDefinitionSet> LoadAll()
		{
			return _multiplicatorDefinitionSets;
		}

		

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IMultiplicatorDefinitionSet> FindAllOvertimeDefinitions()
		{
			return _multiplicatorDefinitionSets.Where(s => s.MultiplicatorType == MultiplicatorType.Overtime).ToList();
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