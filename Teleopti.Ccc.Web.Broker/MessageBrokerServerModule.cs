using Autofac;
using Autofac.Integration.SignalR;
using RegistrationExtensions = Autofac.Integration.Mvc.RegistrationExtensions;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterHubs(typeof(MessageBrokerHub).Assembly);
			RegistrationExtensions.RegisterControllers(builder, typeof(MessageBrokerController).Assembly);

			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();
			builder.RegisterType<SubscriptionPassThrough>().As<IBeforeSubscribe>().SingleInstance();
			builder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>().ExternallyOwned();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance();
		}
	}
}