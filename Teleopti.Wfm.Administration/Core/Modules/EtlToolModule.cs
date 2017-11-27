using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.Administration.Core.Modules
{
	public class EtlToolModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EtlToolJobCollectionModelProvider>().SingleInstance();
			builder.RegisterType<EtlModule.TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
			builder.RegisterType<FindTenantLogonInfoUnsecured>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<PmInfoProvider>().SingleInstance();
		}
	}
}