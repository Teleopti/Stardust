using Autofac;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();

			builder.RegisterType<InProcessMessageSender>().As<IMessageSender>().SingleInstance();
		}
	}
}