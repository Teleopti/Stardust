using System.Threading.Tasks;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class DenormalizeBusBootStrapper : BusBootStrapper
	{
		public DenormalizeBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			Task.Factory.StartNew(() =>
			{
				var initialLoad = new InitialLoadOfScheduleProjectionReadModel(() => Container.Resolve<IServiceBus>(), Container.Resolve<IDataSourceForTenant>());
				initialLoad.Check();
			});
		}
	}
}