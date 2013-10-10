using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScenarioRepository : IScenarioRepository
	{
		private readonly Scenario _scenario;

		public FakeScenarioRepository(Scenario scenario)
		{
			_scenario = scenario;
		}

		public void Add(IScenario entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IScenario entity)
		{
			throw new NotImplementedException();
		}

		public IScenario Get(Guid id)
		{
			return _scenario;
		}

		public IList<IScenario> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IScenario Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IScenario> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public void SetDefault(IScenario myScenario)
		{
			throw new NotImplementedException();
		}

		public IList<IScenario> FindAllSorted()
		{
			throw new NotImplementedException();
		}

		public IList<IScenario> FindEnabledForReportingSorted()
		{
			throw new NotImplementedException();
		}

		public IScenario LoadDefaultScenario()
		{
			throw new NotImplementedException();
		}

		public IScenario LoadDefaultScenario(IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}
	}
}