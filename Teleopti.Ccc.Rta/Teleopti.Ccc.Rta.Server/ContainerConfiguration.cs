using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Rta.Server
{
	public class ContainerConfiguration
	{
		 public ContainerBuilder Configure()
		 {
			 var builder = new ContainerBuilder();
			 var mbCacheModule = new MbCacheModule(null);
			 builder.RegisterModule(mbCacheModule);
			 builder.RegisterModule(new RtaCommonModule(mbCacheModule));
			 return builder;
		 }
	}
}