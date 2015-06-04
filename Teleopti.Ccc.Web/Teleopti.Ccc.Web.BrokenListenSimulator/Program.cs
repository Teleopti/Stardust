using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	class Program
	{
		static void Main(string[] args)
		{
			const string dataSourceName = "Demo";
			var businessUnitId = Guid.NewGuid();
			var scenarioId = Guid.NewGuid();

			var currentDatasource = new FakeCurrentDatasource();
			currentDatasource.FakeName(dataSourceName);

			var businessUnit = new BusinessUnit("..");
			businessUnit.SetId(businessUnitId);
			var currentBusinessUnit = new FakeCurrentBusinessUnit();
			currentBusinessUnit.FakeBusinessUnit(businessUnit);

			var scenario = new Scenario("..");
			scenario.SetId(scenarioId);
			var currentScenario = new FakeCurrentScenario();
			currentScenario.FakeScenario(scenario);

			var builder = new ContainerBuilder();
			var toggleManager = new FakeToggleManager();
			builder.RegisterModule(CommonModule.ForTest(toggleManager));
			builder.RegisterInstance(currentScenario).As<ICurrentScenario>();
			builder.RegisterInstance(currentDatasource).As<ICurrentDataSource>();
			builder.RegisterInstance(currentBusinessUnit).As<ICurrentBusinessUnit>();
			builder.RegisterType<SchedulingScreen>();
			var container = builder.Build();

			var schedulingScreen = container.Resolve<SchedulingScreen>();

			var startDate = "2015-01-05".Utc();
			var endDate = startDate.AddDays(8*7);

			schedulingScreen.Simulate(@"http://localhost:52858/", startDate, endDate);

			Console.WriteLine("started");
			Console.ReadKey();

		}
	}
}
