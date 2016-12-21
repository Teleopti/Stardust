using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.SignalRListenSimulator
{

	public class SimulationData
	{
		public string DataSource { get; set; }
		public Guid BusinessUnit { get; set; }
		public Guid Scenario { get; set; }

		public string Url { get; set; }

		public Guid User { get; set; }
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			var data = new SimulationData
			{
				DataSource = "TeleoptiCCCDemo",
				BusinessUnit = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432"),
				Url = "http://qawfmfarmhost/TeleoptiWFM/Web",
				User = new Guid("11610FE4-0130-4568-97DE-9B5E015B2564")
			};
			Simulate(data, 20, 1000);
		}

		private static void Simulate(SimulationData data, int clients, int screens)
		{

			Console.WriteLine("starting {0} clients with {1} screens each", clients, screens);

			var s = new Stopwatch();
			s.Start();

			var startClients = Enumerable.Range(1, clients).Select(client =>
			{
				return Task.Factory.StartNew(() =>
				{
					Console.WriteLine($"client {client}");

					var builder = new ContainerBuilder();
					var iocArgs = new IocArgs(new ConfigReader())
					{
						MessageBrokerListeningEnabled = true,
						ImplementationTypeForCurrentUnitOfWork = typeof(FromFactory),
					};
					var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
					builder.RegisterModule(new CommonModule(configuration));
					builder.RegisterType<FakeCurrentDatasource>().AsSelf().As<ICurrentDataSource>().SingleInstance();
					builder.RegisterType<FakeCurrentBusinessUnit>().AsSelf().As<ICurrentBusinessUnit>().SingleInstance();
					builder.RegisterType<FakeCurrentScenario>().AsSelf().As<ICurrentScenario>().SingleInstance();
					builder.RegisterType<SimulateMyTimeScreen>().InstancePerDependency();
					var container = builder.Build();

					container.Resolve<FakeCurrentDatasource>().FakeName(data.DataSource);
					container.Resolve<FakeCurrentBusinessUnit>().FakeBusinessUnit(new BusinessUnit("..").WithId(data.BusinessUnit));
					container.Resolve<FakeCurrentScenario>().FakeScenario(new Scenario("..").WithId(data.Scenario));

					container.Resolve<IMessageBrokerUrl>().Configure(data.Url);

					Console.WriteLine($"client.screens {client}");

					// WE DONT NEED IT NOW, BUT THIS PROBABLY NEEDS TO BE SHARED
					var messageBroker = container.Resolve<IMessageBrokerComposite>();
					messageBroker.StartBrokerService(true);
					Console.WriteLine($"starting broker {client}");
					while (!messageBroker.IsAlive)
						Task.Delay(500).Wait();
					Console.WriteLine($"started broker {client}");

					Enumerable.Range(1, screens).ForEach(screen =>
					{
						Console.WriteLine($"starting screen {client}-{screen}");
						container.Resolve<SimulateMyTimeScreen>()
							.Simulate(data, screen, client);
						Console.WriteLine($"started screen {client}-{screen}");
					});

					Console.WriteLine($"/client {client}");
				}, TaskCreationOptions.LongRunning);
			});

			Console.WriteLine($"waiting...");
			Task.WaitAll(startClients.ToArray());
			Console.WriteLine("Started listening, took {0}", s.Elapsed);
			Console.ReadKey();
		}
	}
}
