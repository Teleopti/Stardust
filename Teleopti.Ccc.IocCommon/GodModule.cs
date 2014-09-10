using System.Configuration;
using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.IocCommon
{
	public class GodModule : Module
	{
		public string PathToToggle { get; set; }

		public GodModule()
		{
			PathToToggle = ConfigurationManager.AppSettings["FeatureToggle"];
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
			builder.RegisterType<NewtonsoftJsonDeserializer>().As<IJsonDeserializer>().SingleInstance();
			builder.RegisterModule(new ToggleNetModule(PathToToggle, ConfigurationManager.AppSettings["ToggleMode"]));
			builder.RegisterModule<MessageBrokerModule>();
		}
	}
}