using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerTestAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterModule(new MessageBrokerServerModule(configuration));

			builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();

			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}