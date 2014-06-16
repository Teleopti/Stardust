using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Rta.Server
{
	public static class ContainerBuilder
	{
		 public static Autofac.ContainerBuilder CreateBuilder()
		 {
			 var builder = new Autofac.ContainerBuilder();

			 var mbCacheModule = new MbCacheModule(null);
			 builder.RegisterModule(mbCacheModule);
			 builder.RegisterModule(new RealTimeContainerInstaller(mbCacheModule));
			 return builder;
		 }
	}
}