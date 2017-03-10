using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class PersonAccountModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<TraceableRefreshService>().As<ITraceableRefreshService>();
			builder.RegisterType<PersonAccountUpdater>().As<IPersonAccountUpdater>();
			builder.RegisterType<ClearPersonRelatedInformation>().As<IPersonLeavingUpdater>().SingleInstance();
		}
	}
}