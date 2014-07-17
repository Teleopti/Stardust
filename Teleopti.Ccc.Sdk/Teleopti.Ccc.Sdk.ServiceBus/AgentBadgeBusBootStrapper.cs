using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class AgentBadgeBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			var bus = Container.Resolve<IServiceBus>();
			bus.Send(new AgentBadgeCalculateMessage());
		}
	}
}