using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.Administration.Core.Modules
{
	public class EtlToolModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public EtlToolModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EtlToolJobCollectionModelProvider>().SingleInstance();
			builder.RegisterType<EtlModule.TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
		}
	}
}