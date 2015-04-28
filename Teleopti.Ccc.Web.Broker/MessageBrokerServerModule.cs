using Autofac;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>().ExternallyOwned();
			builder.RegisterType<SubscriptionPassThrough>().As<IBeforeSubscribe>();
		}
	}
}