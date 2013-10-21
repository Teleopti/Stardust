using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ScenarioConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string BusinessUnit { get; set; }
		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			var businessUnit = businessUnitRepository.LoadAll().Single(b => b.Name == BusinessUnit);

			var scenario = ScenarioFactory.CreateScenarioAggregate(Name, true);
			scenario.SetBusinessUnit(businessUnit);
			new ScenarioRepository(uow).Add(scenario);
		}
	}
}