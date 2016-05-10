using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsScenarioRepository
	{
		void SetName(Guid scenarioCode, string name, Guid businessUnitCode);
		void AddScenario(AnalyticsScenario scenario);
		IList<AnalyticsScenario> Scenarios();
	}
}