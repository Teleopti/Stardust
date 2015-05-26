using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{

		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder,configuration);
			builder.RegisterModule<MessageBrokerServerModule>();
			builder.RegisterType<MailboxRepository>()
				.As<IMailboxRepository>()
				.SingleInstance()
				.ApplyAspects();
		}

		protected override void BeforeTest()
		{
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnBeforeInvocation(null);
			SetupFixtureForAssembly.BackupAnalyticsDatabase();
		}
		
		protected override void AfterTest()
		{
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnAfterInvocation(null, null);
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}