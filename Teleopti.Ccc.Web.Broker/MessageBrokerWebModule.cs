using Autofac;
using Autofac.Integration.SignalR;
using RegistrationExtensions = Autofac.Integration.Mvc.RegistrationExtensions;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterHubs(typeof(MessageBrokerHub).Assembly);
			RegistrationExtensions.RegisterControllers(builder, typeof(MessageBrokerController).Assembly);
		}
	}
}