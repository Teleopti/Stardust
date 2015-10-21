using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Web.Core.MessageBroker
{
	public class MessageBrokerWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();
		}
	}
}