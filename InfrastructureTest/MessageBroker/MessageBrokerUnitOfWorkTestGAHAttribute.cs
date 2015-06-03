using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestGAHAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder,configuration);

			builder.RegisterModule(new MessageBrokerServerModule(configuration));

			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
		}

		protected override void BeforeTest()
		{
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnBeforeInvocation(null);
		}
		
		protected override void AfterTest()
		{
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnAfterInvocation(null, null);
		}
	}
}