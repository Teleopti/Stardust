using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ScenarioConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string BusinessUnit { get; set; }
		public bool EnableReporting { get; set; }

		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			Scenario = ScenarioFactory.CreateScenario(Name, true, EnableReporting);

			var businessUnit = new BusinessUnitRepository(uow).LoadAll().Single(b => b.Name == BusinessUnit);
			Scenario.SetBusinessUnit(businessUnit);

			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}