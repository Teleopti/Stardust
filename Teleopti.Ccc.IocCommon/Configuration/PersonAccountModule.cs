using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class PersonAccountModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<TraceableRefreshService>().As<ITraceableRefreshService>();
		}
	}
}