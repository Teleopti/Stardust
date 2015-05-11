using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	public class MessageBrokerServerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			//builder.RegisterModule<MessageBrokerWebModule>();
			builder.RegisterModule<MessageBrokerServerModule>();
			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
			builder.RegisterInstance(new ActionImmediate()).As<IActionScheduler>();
		}
	}
}