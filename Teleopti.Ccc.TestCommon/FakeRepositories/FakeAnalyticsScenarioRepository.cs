using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsScenarioRepository : IAnalyticsScenarioRepository
	{
		private readonly IList<AnalyticsScenario> fakeScenarios;

		public FakeAnalyticsScenarioRepository()
		{
			fakeScenarios = new List<AnalyticsScenario>();
		}
		
		public void SetName(Guid scenarioCode, string name, Guid businessUnitCode)
		{
			fakeScenarios.First(a => a.ScenarioCode == scenarioCode && 
			a.BusinessUnitCode == businessUnitCode).ScenarioName = name;
		}

		public void AddScenario(AnalyticsScenario scenario)
		{
			scenario.ScenarioId = fakeScenarios.Any() ? fakeScenarios.Max(a => a.ScenarioId) + 1 : 1;
			fakeScenarios.Add(scenario);
		}

		public IList<AnalyticsScenario> Scenarios()
		{
			return fakeScenarios;
		}
	}
}