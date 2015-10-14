using System.Threading.Tasks;
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
			base.OnEndStart();

			Task.Run(() =>
			{
				var initialLoad = new InitialLoadOfScheduleProjectionReadModel(() => Container.Resolve<IServiceBus>(), Container.Resolve<DataSourceForTenantWrapper>().DataSource()());
				initialLoad.Check();
			});
		}
	}
}