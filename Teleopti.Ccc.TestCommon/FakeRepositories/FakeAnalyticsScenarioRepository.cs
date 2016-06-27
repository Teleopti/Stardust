using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsScenarioRepository : IAnalyticsScenarioRepository
	{
		private readonly List<AnalyticsScenario> fakeScenarios;

		public FakeAnalyticsScenarioRepository()
		{
			fakeScenarios = new List<AnalyticsScenario>();
		}
		
		public void AddScenario(AnalyticsScenario scenario)
		{
			if (fakeScenarios.Any(x => x.ScenarioId == scenario.ScenarioId))
				scenario.ScenarioId = fakeScenarios.Max(a => a.ScenarioId) + 1;
			fakeScenarios.Add(scenario);
		}

		public void UpdateScenario(AnalyticsScenario scenario)
		{
			scenario.ScenarioId = fakeScenarios.First(a => a.ScenarioCode == scenario.ScenarioCode).ScenarioId;
			fakeScenarios.RemoveAll(a => a.ScenarioCode == scenario.ScenarioCode);
			fakeScenarios.Add(scenario);
		}

		public IList<AnalyticsScenario> Scenarios()
		{
			return fakeScenarios;
		}

		public AnalyticsScenario Get(Guid scenarioCode)
		{
			return fakeScenarios.FirstOrDefault(x => x.ScenarioCode == scenarioCode);
		}
	}
}