using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScenarioRepository : IScenarioRepository
	{
		private readonly List<IScenario> _scenario = new List<IScenario>();

		public FakeScenarioRepository()
		{			
		}

		public FakeScenarioRepository(IScenario scenario)
		{
			_scenario.Add(scenario);
		}

		public void Has(IScenario scenario)
		{
			var existing = _scenario.SingleOrDefault(x => x.Id.Equals(scenario.Id));
			if(existing != null) return;
			_scenario.Add(scenario);
		}

		public IScenario Has(string name)
		{
			var scenario = new Scenario(name).WithId();
			scenario.DefaultScenario = true;
			Has(scenario);
			return scenario;
		}
		
		public IScenario Has()
		{
			return Has("_");
		}

		public void Add(IScenario entity)
		{
			_scenario.Add(entity.WithIdIfNotPresent());
		}

		public void Remove(IScenario entity)
		{
			_scenario.Remove(entity);
		}

		public IScenario Get(Guid id)
		{
			return _scenario.FirstOrDefault(s => s.Id == id);
		}

		public IEnumerable<IScenario> LoadAll()
		{
			return _scenario;
		}

		public IScenario Load(Guid id)
		{
			return Get(id);
		}

		public IList<IScenario> FindAllSorted()
		{
			return _scenario.OrderByDescending(s => s.DefaultScenario).ThenBy(s => s.Description.Name).ToArray();
		}

		public IList<IScenario> FindEnabledForReportingSorted()
		{
			return _scenario.OrderByDescending(s => s.DefaultScenario).ThenBy(s => s.Description.Name).Where(s => s.EnableReporting).ToArray();
		}

		public IScenario LoadDefaultScenario()
		{
			var defaultScenario = _scenario.SingleOrDefault(s => s.DefaultScenario);
			if (defaultScenario == null)
				throw new NoDefaultScenarioException();
			return defaultScenario;
		}

		public IScenario LoadDefaultScenario(IBusinessUnit businessUnit)
		{
			return _scenario.SingleOrDefault(s => s.DefaultScenario && s.BusinessUnit == businessUnit);
		}
	}
}