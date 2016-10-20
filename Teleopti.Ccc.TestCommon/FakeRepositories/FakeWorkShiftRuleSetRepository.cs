using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkShiftRuleSetRepository : IWorkShiftRuleSetRepository
	{
		private readonly IList<IWorkShiftRuleSet> workShiftRuleSets = new List<IWorkShiftRuleSet>();
		public void Add(IWorkShiftRuleSet root)
		{
			workShiftRuleSets.Add(root);
		}

		public void Remove(IWorkShiftRuleSet root)
		{
			throw new NotImplementedException();
		}

		public IWorkShiftRuleSet Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IWorkShiftRuleSet> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IWorkShiftRuleSet Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IWorkShiftRuleSet> FindAllWithLimitersAndExtenders()
		{
			throw new NotImplementedException();
		}
	}
}