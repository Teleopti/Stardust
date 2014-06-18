using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Server;

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
			builder.RegisterModule(new RealTimeContainerInstaller(_mbCacheModule));
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
		}
	}
}