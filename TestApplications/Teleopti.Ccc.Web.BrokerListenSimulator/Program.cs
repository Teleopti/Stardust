using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokerListenSimulator
{

	public class SimulationData
	{
		public string DataSource { get; set; }
		public Guid BusinessUnit { get; set; }
		public Guid Scenario { get; set; }

		public string Url { get; set; }

		public DateTime SchedulingScreenStartDate { get; set; }
		public DateTime SchedulingScreenEndDate { get; set; }
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			var data = new SimulationData
			{
				DataSource = "Teleopti WFM",
				BusinessUnit = Guid.Parse("C087E9B6-5F52-4969-968A-A24200ACE742"),
				Scenario = Guid.Parse("3348D359-D15F-4069-9277-A24200ACEB3D"),
				Url = "http://wfmrc2/TeleoptiWFM/Web",
				SchedulingScreenStartDate = "2016-01-01 00:00".Utc(),
				SchedulingScreenEndDate = "2016-12-31 00:00".Utc()
			};
			Simulate(data, 200, 2);
		}
		
		private static void Simulate(SimulationData data, int clients, int screens)
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
				};
				var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
				builder.RegisterModule(new CommonModule(configuration));
				builder.RegisterInstance(currentScenario).As<ICurrentScenario>();
				builder.RegisterInstance(currentDatasource).As<ICurrentDataSource>();
				builder.RegisterInstance(currentBusinessUnit).As<ICurrentBusinessUnit>();
				builder.RegisterType<SimulateSchedulingScreen>().InstancePerDependency();
				var container = builder.Build();

				container.Resolve<IMessageBrokerUrl>()
					.Configure(data.Url);

				var messageBroker = container.Resolve<IMessageBrokerComposite>();
				messageBroker.StartBrokerService(true);
				while (!messageBroker.IsAlive)
					Task.Delay(500);

				var screenSimulations = Enumerable.Range(1, screens).Select(screen => {
					var schedulingScreen = container.Resolve<SimulateSchedulingScreen>();
					return Task.Factory.StartNew(() =>
					{
						schedulingScreen.Simulate(data, screen, client);
					}, TaskCreationOptions.LongRunning);
				});

				Task.WaitAll(screenSimulations.ToArray());
			});

			Console.WriteLine("Started listening, took {0}", s.Elapsed);
			Console.ReadKey();
		}
	}
}
