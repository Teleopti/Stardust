using log4net;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultScenario : IHashableDataSetup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultScenario));

		public static IScenario Scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			new ScenarioRepository(currentUnitOfWork).Add(Scenario);
		}

		public int HashValue()
		{
			log.Debug("Scenario.Description.Name.GetHashCode() " + Scenario.Description.Name.GetHashCode());
			return Scenario.Description.Name.GetHashCode();
		}
	}
}