using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.IoC
{
	public class RtaAreaModule : Module
	{
		private readonly MbCacheModule _mbCacheModule;
		private readonly IIocConfiguration _configuration;

		public RtaAreaModule(MbCacheModule mbCacheModule, IIocConfiguration configuration)
		{
			_mbCacheModule = mbCacheModule;
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new RtaCommonModule(_mbCacheModule, _configuration));
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
		}
	}
}