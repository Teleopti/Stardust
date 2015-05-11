using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>().ExternallyOwned();
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();
		}
	}
}