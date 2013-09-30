using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class PersonAccountModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<TraceableRefreshService>().As<ITraceableRefreshService>();
			builder.RegisterType<PersonAccountUpdater>().As<IPersonAccountUpdater>();
		}
	}
}