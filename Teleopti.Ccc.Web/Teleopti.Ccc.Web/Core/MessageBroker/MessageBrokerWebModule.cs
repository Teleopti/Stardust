using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();
		}
	}
}