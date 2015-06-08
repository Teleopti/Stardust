﻿using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
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
			const string dataSourceName = "Teleopti WFM";
			var businessUnitId = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA");
			var scenarioId = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432");
			var startDate = "2015-06-08".Utc();
			var endDate = "2015-08-03".Utc();
			const string brokerUrl = @"http://localhost:52858/";



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
			builder.RegisterType<SimulatedSchedulingScreen>().InstancePerDependency();
			var container = builder.Build();


			Console.WriteLine("starting...");

			Enumerable.Range(1, (200 + 5 + 2)).ForEach(i =>
			{
				var schedulingScreen = container.Resolve<SimulatedSchedulingScreen>();
				
				schedulingScreen.Simulate(i, brokerUrl, startDate, endDate);
				Console.WriteLine("#" + i + "starting");
			});

			Console.ReadKey();

		}
	}
}
