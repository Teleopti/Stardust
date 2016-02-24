using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.BrokenListenSimulator.ListenSimulators;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;
using Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var myTimeData = new MyTimeData
			{
				BaseUrl = @"https://teleoptiscaleout.teleopticloud.com/Web/",
				User = new Guid("10957AD5-5489-48E0-959A-9B5E015B2B5C"),
				AbsenseId = new Guid("07d71222-2525-4efc-8129-a474009bd03b"),
				DataSource = "Teleopti WFM",
				BusinessUnit = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Scenario = new Guid("E21D813C-238C-4C3F-9B49-9B5E015AB432"),
				BusinessUnitName = "Teleopti WFM Demo"

				//BaseUrl = @"http://qawfmfarmhost/TeleoptiWFM/Web/",
				//User = new Guid("9d42c9bf-f766-473f-970c-9b5e015b2564"),
				//AbsenseId = new Guid("435CC5C8-89C0-4714-96FD-9B5E015AB330"),
				//DataSource = "Teleopti WFM",
				//BusinessUnit = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				//Scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432"),
				//BusinessUnitName = "TeleoptiCCCDemo"

				//BaseUrl = "http://teleopti745/TeleoptiWFM/Web/",
				//User = new Guid("9d42c9bf-f766-473f-970c-9b5e015b2564"),
				//DataSource = "Teleopti WFM",
				//AbsenseId = new Guid("4b4c15f0-5c3c-479e-8f9f-9bb900b80624"),
				//BusinessUnit = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				//Scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432")
			};


			if (args.Any()&&args[0] == "-c")
			{
				// start trafic simulator
				var builder = new ContainerBuilder();
				builder.RegisterAssemblyTypes(typeof (TrafficSimulatorBase<>).Assembly)
					.Where(t => t.IsClosedTypeOf(typeof (TrafficSimulatorBase<>)))
					.AsClosedTypesOf(typeof (TrafficSimulatorBase<>)).InstancePerDependency();
				var container = builder.Build();
				TrafficSimulatorBase<MyTimeData> trafficSimulator;

				var sw = new Stopwatch();
				sw.Start();

				if (container.TryResolve(out trafficSimulator))
				{
					var caseNumber = "1";
					if (args.Count() > 1 )
					{
						caseNumber = args[1];
					}
					switch (caseNumber)
					{
						case "1":
							simulateTrafficCase1(trafficSimulator, myTimeData);
							break;
						case "2":
							simulateTrafficCase2(trafficSimulator, myTimeData);
							break;
						default:
							simulateTrafficCase1(trafficSimulator, myTimeData);
							break;
					}
				}
				Console.WriteLine("Done simulating traffic, took {0}", sw.Elapsed);
			}
			else if (args.Any() && args[0] == "-p")
			{
				myTimeData.User = new Guid(args[1]);
				simulate(myTimeData, 1, 100);
			}
			else if (args.Any() && args[0] == "-s")
			{
				var builder = new ContainerBuilder();
				builder.RegisterAssemblyTypes(typeof(TrafficSimulatorBase<>).Assembly)
					.Where(t => t.IsClosedTypeOf(typeof(TrafficSimulatorBase<>)))
					.AsClosedTypesOf(typeof(TrafficSimulatorBase<>)).InstancePerDependency();
				var container = builder.Build();
				TrafficSimulatorBase<MyTimeData> trafficSimulator;
				if (container.TryResolve(out trafficSimulator))
				{
					trafficSimulator.Start(myTimeData.BaseUrl, myTimeData.BusinessUnitName, null, null);
					var people = trafficSimulator.GetPeopleForPartTimePercentaget100(DateTime.Today);
					foreach (var person in people)
					{
						var startInfo = new ProcessStartInfo
						{
							FileName = typeof(Program).Assembly.GetName().Name+".exe",
							Arguments = "-p " + person
						};
						var process = new Process { StartInfo = startInfo };
						process.Start();
						Thread.Sleep(200);
					}
				}
			}
			else
			{
				// start listen simulator
				simulate(myTimeData, 1, 100);//200 + 5 + 2;
			}
		}

		private static void simulateTrafficCase1<T>(TrafficSimulatorBase<T> trafficSimulator, T data) where T : SimulationDataBase
		{
			// for mytime screen, if one person is subscribed, expected number of notifications should 10 * 2
			trafficSimulator.Start(data.BaseUrl, data.BusinessUnitName, null, null);
			trafficSimulator.AddFullDayAbsenceForAllPeopleWithPartTimePercentage100ByNextNDays(data, 10);
		}

		private static void simulateTrafficCase2<T>(TrafficSimulatorBase<T> trafficSimulator, T data) where T : SimulationDataBase
		{
			// for mytime screen, if one person is subscribed, expected number of notifications should 3 * 2
			trafficSimulator.Start(data.BaseUrl, data.BusinessUnitName, null, null);
			trafficSimulator.AddFullDayAbsenceForThePersonByNextNDays(data, 3);
		}

		private static void simulate<T>(T data, int clients, int screens) where T : SimulationDataBase
		{
			var currentDatasource = new FakeCurrentDatasource(new DataSourceState());
			currentDatasource.FakeName(data.DataSource);

			var businessUnit = new BusinessUnit("..");
			businessUnit.SetId(data.BusinessUnit);
			var currentBusinessUnit = new FakeCurrentBusinessUnit();
			currentBusinessUnit.FakeBusinessUnit(businessUnit);

			var scenario = new Scenario("..");
			scenario.SetId(data.Scenario);
			var currentScenario = new FakeCurrentScenario();
			currentScenario.FakeScenario(scenario);

			var stats = new Stats(() => { });
			Console.WriteLine("starting {0} clients with {1} screens each, watching on {2}", clients, screens, typeof(T).Name);

			var s = new Stopwatch();
			s.Start();
			Enumerable.Range(1, clients).ForEach(client =>
			{
				var builder = new ContainerBuilder();
				var iocArgs = new IocArgs(new ConfigReader())
				{
					MessageBrokerListeningEnabled = true,
					ImplementationTypeForCurrentUnitOfWork = typeof (FromFactory),
					ThrottleMessages = false
				};
				var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
				builder.RegisterModule(new CommonModule(configuration));
				builder.RegisterInstance(currentScenario).As<ICurrentScenario>();
				builder.RegisterInstance(currentDatasource).As<ICurrentDataSource>();
				builder.RegisterInstance(currentBusinessUnit).As<ICurrentBusinessUnit>();
				builder.RegisterType(typeof (TrafficSimulatorBase<>)).InstancePerDependency();
				builder.RegisterAssemblyTypes(typeof (SimulateBase<>).Assembly)
					.Where(t => t.IsClosedTypeOf(typeof (SimulateBase<>)))
					.AsClosedTypesOf(typeof (SimulateBase<>)).InstancePerDependency();


				var container = builder.Build();

				var url = container.Resolve<IMessageBrokerUrl>();
				url.Configure(data.BaseUrl);

				var messageBroker = container.Resolve<IMessageBrokerComposite>();
				messageBroker.StartBrokerService(true);

				while (!messageBroker.IsAlive)
					Task.Delay(500);

				Enumerable.Range(1, screens).ForEach(screen =>
				{
					var schedulingScreen = container.Resolve<SimulateBase<T>>();
					schedulingScreen.Simulate(data, screen, client, stats.Callback);
				});

			});
			Console.WriteLine("Done subscribing, took {0}", s.Elapsed);
			stats.Stopwatch.Start();
			while (stats.Stopwatch.IsRunning && stats.Stopwatch.Elapsed < TimeSpan.FromSeconds(600))
			{
				Thread.Sleep(TimeSpan.FromSeconds(5));
				Console.WriteLine("Current received messages: {0}, elapsed {1}", stats.NumberOfCallbacks, stats.Stopwatch.Elapsed);
			}
			Console.WriteLine();
			Console.WriteLine("Receiving messages took {0}", stats.Stopwatch.Elapsed);
			Console.WriteLine(stats.NumberOfCallbacks);
			Console.ReadKey();
		}
	}
}
