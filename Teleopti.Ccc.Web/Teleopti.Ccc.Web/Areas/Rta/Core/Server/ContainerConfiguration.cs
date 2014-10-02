using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ContainerConfiguration
	{
		 public ContainerBuilder Configure()
		 {
			 var builder = new ContainerBuilder();
			 var mbCacheModule = new MbCacheModule(null);
			 builder.RegisterModule<CommonModule>();
			 builder.RegisterModule(mbCacheModule);
			 builder.RegisterModule(new RtaCommonModule(mbCacheModule));
			 return builder;
		 }
	}
}