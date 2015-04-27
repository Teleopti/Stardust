using Autofac;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();
			builder.RegisterType<SubscriptionPassThrough>().As<IBeforeSubscribe>().SingleInstance();
			builder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>().ExternallyOwned();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance();
		}
	}
}