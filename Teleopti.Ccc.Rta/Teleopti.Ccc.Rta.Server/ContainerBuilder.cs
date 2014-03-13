using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Rta.Server
{
	public static class ContainerBuilder
	{
		 public static Autofac.ContainerBuilder CreateBuilder()
		 {
			 var builder = new Autofac.ContainerBuilder();

			 var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			 builder.RegisterModule(mbCacheModule);
			 builder.RegisterModule(new RealTimeContainerInstaller(mbCacheModule));
			 return builder;
		 }
	}
}