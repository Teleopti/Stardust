using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
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
		static void Main(string[] args)
		{
            
            var myTimeData = new MyTimeData
            {
                User = new Guid("9d42c9bf-f766-473f-970c-9b5e015b2564")
            };
		    simulate(myTimeData);

		}

        private static void simulate<T>(T data)
	    {
            const string dataSourceName = "Teleopti WFM";
            var businessUnitId = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA");
            var scenarioId = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432");
            var startDate = "2015-11-02".Utc();
            var endDate = "2015-11-29".Utc();
            //const string brokerUrl = @"http://localhost:52858/";
            //const string brokerUrl = @"http://localhost/TeleoptiWFM/Web/";
            //const string brokerUrl = @"http://wfmrc1/TeleoptiWFM/Web/";
            const string brokerUrl = @"http://teleopti745/TeleoptiWFM/Web/";

            var currentDatasource = new FakeCurrentDatasource(new DataSourceState());
            currentDatasource.FakeName(dataSourceName);

            var businessUnit = new BusinessUnit("..");
            businessUnit.SetId(businessUnitId);
            var currentBusinessUnit = new FakeCurrentBusinessUnit();
            currentBusinessUnit.FakeBusinessUnit(businessUnit);

            var scenario = new Scenario("..");
            scenario.SetId(scenarioId);
            var currentScenario = new FakeCurrentScenario();
            currentScenario.FakeScenario(scenario);

            var clients = 4;
            var screens = 20000; //200 + 5 + 2;
            var updates = 2;
            var subscriptionsToEvent = 2;
            var stats = new Stats(screens * clients * updates * subscriptionsToEvent);
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
                url.Configure(brokerUrl);

                var messageBroker = container.Resolve<IMessageBrokerComposite>();
                messageBroker.StartBrokerService(true);

                while (!messageBroker.IsAlive)
                    Task.Delay(500);

                Enumerable.Range(1, screens).ForEach(screen =>
                {
                    var schedulingScreen = container.Resolve<SimulateBase<T>>();
                    schedulingScreen.Simulate(data, screen, client, stats.Callback);

                    //Console.WriteLine("client/screen " + client + "/" + screen);
                });

            });
            var builder2 = new ContainerBuilder();
            builder2.RegisterAssemblyTypes(typeof(TrafficSimulatorBase<>).Assembly)
                    .Where(t => t.IsClosedTypeOf(typeof(TrafficSimulatorBase<>)))
                    .AsClosedTypesOf(typeof(TrafficSimulatorBase<>)).InstancePerDependency();
            var container2 = builder2.Build();
            TrafficSimulatorBase<T> trafficSimulator;
            if (container2.TryResolve(out trafficSimulator))
            {
                trafficSimulator.Start(brokerUrl, null, null, null);
                trafficSimulator.Simulate(data);
            }
            stats.Stopwatch.Start();
            Console.WriteLine("Done subscribing and simulating traffic, took {0}", s.Elapsed);
            
            while (stats.Stopwatch.IsRunning && stats.Stopwatch.Elapsed < TimeSpan.FromSeconds(60))
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

    public class Stats
    {
        public int NumberOfCallbacks;
        public int NumberExpected;
        public Stopwatch Stopwatch = new Stopwatch();
        private object thisLock = new object();

        public Stats(int expected)
        {
            NumberExpected = expected;
        }
        public void Callback(object sender, EventMessageArgs e)
        {
            lock (thisLock)
            {
                if (NumberOfCallbacks%500 == 0) Console.Write(".");
                NumberOfCallbacks++;
                if (NumberOfCallbacks == NumberExpected)
                    Stopwatch.Stop();
            }
        }
    }
}
