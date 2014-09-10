using System.Configuration;
using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces;

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
			 builder.RegisterModule(new ToggleNetModule(ConfigurationManager.AppSettings["FeatureToggle"], ConfigurationManager.AppSettings["ToggleMode"]));
			 builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
			 builder.RegisterType<NewtonsoftJsonDeserializer>().As<IJsonDeserializer>().SingleInstance();
			 builder.RegisterModule<MessageBrokerModule>();
			 return builder;
		 }
	}
}