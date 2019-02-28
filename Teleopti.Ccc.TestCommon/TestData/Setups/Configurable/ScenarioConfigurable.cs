using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ScenarioConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string BusinessUnit { get; set; }
		public bool EnableReporting { get; set; }
		public bool ExtraScenario { get; set; }

		public IScenario Scenario;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			Scenario = ScenarioFactory.CreateScenario(Name, !ExtraScenario, EnableReporting);

			var businessUnit = BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().Single(b => b.Name == BusinessUnit);
			Scenario.SetBusinessUnit(businessUnit);

			ScenarioRepository.DONT_USE_CTOR(currentUnitOfWork).Add(Scenario);
		}
	}
}