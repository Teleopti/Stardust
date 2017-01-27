using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ResourcePlannerTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));

			system.UseTestDouble<FakeExistingForecastRepository>().For<IExistingForecastRepository>();

			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}