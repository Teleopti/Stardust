using Autofac;
using Rhino.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class DenormalizeBusBootStrapper : BusBootStrapper
	{
		public DenormalizeBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			var initialLoad = new InitialLoadOfScheduleProjectionReadModel(() => Container.Resolve<IServiceBus>());
			initialLoad.Check();
		}
	}
}