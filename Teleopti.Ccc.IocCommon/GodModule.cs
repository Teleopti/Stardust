using System.Configuration;
using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class GodModule : Module
	{
		public string PathToToggle { get; set; }
		public string ToggleMode { get; set; }
		public bool MessageBrokerListeningEnabled { get; set; }

		public GodModule()
		{
			PathToToggle = ConfigurationManager.AppSettings["FeatureToggle"];
			ToggleMode = ConfigurationManager.AppSettings["ToggleMode"];
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<JsonSerializationModule>();
			builder.RegisterModule(new ToggleNetModule(PathToToggle, ToggleMode));
			builder.RegisterModule(new MessageBrokerModule {MessageBrokerListeningEnabled = MessageBrokerListeningEnabled});
		}
	}
}