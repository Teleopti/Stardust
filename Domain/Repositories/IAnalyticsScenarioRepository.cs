using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsScenarioRepository
	{
		void AddScenario(AnalyticsScenario scenario);
		void UpdateScenario(AnalyticsScenario scenario);
		IList<AnalyticsScenario> Scenarios();
	}
}