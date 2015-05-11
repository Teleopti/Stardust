using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	public class MessageBrokerServerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule<MessageBrokerWebModule>();
			builder.RegisterModule<MessageBrokerServerModule>();
			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
			builder.RegisterInstance(new ActionImmediate()).As<IActionScheduler>();
		}
	}
}