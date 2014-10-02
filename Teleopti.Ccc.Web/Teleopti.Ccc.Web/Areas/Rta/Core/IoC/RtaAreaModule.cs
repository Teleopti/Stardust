using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.IoC
{
	public class RtaAreaModule : Module
	{
		private readonly MbCacheModule _mbCacheModule;

		public RtaAreaModule(MbCacheModule mbCacheModule)
		{
			_mbCacheModule = mbCacheModule;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new RtaCommonModule(_mbCacheModule));
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
		}
	}
}