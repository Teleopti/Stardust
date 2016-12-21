using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.SignalRLoadGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var businessUnitId = Guid.Parse("928DD0BC-BF40-412E-B970-9B5E015AADEA");
			var personId = new Guid("11610FE4-0130-4568-97DE-9B5E015B2564");
			var scenario = Guid.Parse("E21D813C-238C-4C3F-9B49-9B5E015AB432");
			var serverUrl = "http://qawfmfarmhost/TeleoptiWFM/Web";
			var datasource = "TeleoptiCCCDemo";

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
			var container = builder.Build();

			container.Resolve<FakeCurrentDatasource>().FakeName(datasource);
			container.Resolve<FakeCurrentBusinessUnit>().FakeBusinessUnit(new BusinessUnit("..").WithId(businessUnitId));
			container.Resolve<FakeCurrentScenario>().FakeScenario(new Scenario("..").WithId(scenario));

			var url = container.Resolve<IMessageBrokerUrl>();
			url.Configure(serverUrl);

			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			messageBroker.StartBrokerService(true);

			while (!messageBroker.IsAlive)
				Task.Delay(500).Wait();

			10.Times(() =>
			{
				Console.WriteLine("Sending message!");
				messageBroker.Send(datasource, businessUnitId, DateTime.Today, DateTime.Today,
						Guid.Empty, personId, typeof(Person), Guid.Empty, typeof(IScheduleChangedInDefaultScenario),
						DomainUpdateType.NotApplicable, null);
				Thread.Sleep(TimeSpan.FromSeconds(10));
			});
		}
	}
}
