using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.LogClientProxy.TeleoptiRtaService;

namespace Teleopti.Ccc.Rta.Server
{
	public static class BuildContainer
	{
		 public static IContainer Build()
		 {
			 var builder = new ContainerBuilder();

			 var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			 builder.RegisterType<TeleoptiRtaService>().SingleInstance();
			 builder.RegisterModule(mbCacheModule);
			 builder.RegisterModule(new RealTimeContainerInstaller(mbCacheModule));
			 return builder.Build();
		 }
	}
}