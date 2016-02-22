using System;
using System.Diagnostics;
using System.Linq;
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
    class Program
    {
		private const int clients = 1;
		private const int screens = 3000; //200 + 5 + 2;
		private const int updates = 1;
		private const int subscriptionsToEvent = 2;

	    static void Main(string[] args)
        {
            var myTimeData = new MyTimeData
            {
				BaseUrl = @"http://qawfmfarmhost/TeleoptiWFM/Web/",
				User = new Guid("9d42c9bf-f766-473f-970c-9b5e015b2564"),
				AbsenseId = new Guid("435CC5C8-89C0-4714-96FD-9B5E015AB330"),
				DataSource = "Teleopti WFM",
				BusinessUnit = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432"),
				BusinessUnitName = "TeleoptiCCCDemo"

				//BaseUrl = "http://teleopti745/TeleoptiWFM/Web/",
				//User = new Guid("9d42c9bf-f766-473f-970c-9b5e015b2564"),
				//DataSource = "Teleopti WFM",
				//AbsenseId = new Guid("4b4c15f0-5c3c-479e-8f9f-9bb900b80624"),
				//BusinessUnit = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				//Scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432")
            };
            simulate(myTimeData);

        }

        private static void simulate<T>(T data) where T:SimulationDataBase
        {
            var builder2 = new ContainerBuilder();
            builder2.RegisterAssemblyTypes(typeof(TrafficSimulatorBase<>).Assembly)
                    .Where(t => t.IsClosedTypeOf(typeof(TrafficSimulatorBase<>)))
                    .AsClosedTypesOf(typeof(TrafficSimulatorBase<>)).InstancePerDependency();
            var container2 = builder2.Build();
            TrafficSimulatorBase<T> trafficSimulator;
            if (container2.TryResolve(out trafficSimulator))
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
                
                var stats = new Stats(screens * clients * updates * subscriptionsToEvent, trafficSimulator.CallbackAction);
                Console.WriteLine("starting {0} clients with {1} screens each", clients, screens);

                var s = new Stopwatch();
                s.Start();
                Enumerable.Range(1, clients).ForEach(client =>
                {
                    var builder = new ContainerBuilder();
                    var iocArgs = new IocArgs(new ConfigReader())
                    {
                        MessageBrokerListeningEnabled = true,
                        ImplementationTypeForCurrentUnitOfWork = typeof(FromFactory),
                        ThrottleMessages = false
                    };
                    var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
                    builder.RegisterModule(new CommonModule(configuration));
                    builder.RegisterInstance(currentScenario).As<ICurrentScenario>();
                    builder.RegisterInstance(currentDatasource).As<ICurrentDataSource>();
                    builder.RegisterInstance(currentBusinessUnit).As<ICurrentBusinessUnit>();
                    builder.RegisterType(typeof(TrafficSimulatorBase<>)).InstancePerDependency();
                    builder.RegisterAssemblyTypes(typeof(SimulateBase<>).Assembly)
                        .Where(t => t.IsClosedTypeOf(typeof(SimulateBase<>)))
                        .AsClosedTypesOf(typeof(SimulateBase<>)).InstancePerDependency();


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

                trafficSimulator.Start(data.BaseUrl, data.BusinessUnitName, null, null);
                trafficSimulator.Simulate(data);

                stats.Stopwatch.Start();
                Console.WriteLine("Done simulating traffic, took {0}", s.Elapsed);

                while (stats.Stopwatch.IsRunning && stats.Stopwatch.Elapsed < TimeSpan.FromSeconds(600))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
                Console.WriteLine();
                if (stats.Stopwatch.IsRunning)
                {
                    Console.WriteLine("Missed messages");
                }
                else
                {
                    Console.WriteLine("Receiving messages took {0}", stats.Stopwatch.Elapsed);
                }
                Console.WriteLine(stats.NumberOfCallbacks);
                Console.ReadKey();
            }
        }

    }
}
