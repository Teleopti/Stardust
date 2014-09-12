using System;
using System.Configuration;
using Autofac;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon
{
	public class CommonModule : Module
	{
		public string PathToToggle { get; set; }
		public string ToggleMode { get; set; }
		public bool MessageBrokerListeningEnabled { get; set; }
		public Type RepositoryConstructorType { get; set; }

		public CommonModule()
		{
			PathToToggle = ConfigurationManager.AppSettings["FeatureToggle"];
			ToggleMode = ConfigurationManager.AppSettings["ToggleMode"];
			RepositoryConstructorType = typeof(ICurrentUnitOfWork);
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<JsonSerializationModule>();
			builder.RegisterModule(new ToggleNetModule(PathToToggle, ToggleMode));
			builder.RegisterModule(new MessageBrokerModule {MessageBrokerListeningEnabled = MessageBrokerListeningEnabled});
			builder.RegisterModule(new RepositoryModule {RepositoryConstructorType = RepositoryConstructorType});
		}
	}
}