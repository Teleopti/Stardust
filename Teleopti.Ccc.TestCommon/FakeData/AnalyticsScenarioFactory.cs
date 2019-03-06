using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class AnalyticsScenarioFactory
	{
		public static AnalyticsScenario CreateAnalyticsScenario(IScenario scenario)
		{
			return CreateAnalyticsScenario(scenario, 1);
		}

		public static AnalyticsScenario CreateAnalyticsScenario(IScenario scenario, int businessUnitId)
		{
			return new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = scenario.Description.Name,
				ScenarioId = 1,
				BusinessUnitCode = scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				BusinessUnitId = businessUnitId
			};
		}
	}
}