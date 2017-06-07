using System;
using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon
{
	public class IocArgs
	{
		public string FeatureToggle { get; set; }
		public string ToggleMode { get; set; }
		public string TenantServer { get; }
		public string ConfigServer { get; }
		public string ReportServer { get; }
		public string MatrixWebSiteUrl { get; }
		public bool DisableWebSocketCors { get; }
		public bool OptimizeScheduleChangedEvents_DontUseFromWeb { get; set; }
		public string MessageBrokerUrl { get; }

		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }
		public IDataSourceConfigurationSetter DataSourceConfigurationSetter { get; set; }
		public Type ImplementationTypeForCurrentUnitOfWork { get; set; }
		public bool CacheRulesetPerLifeTimeScope { get; set; } = true;

		public bool BehaviorTestServer { get; set; }
		public bool AllEventPublishingsAsSync { get; set; }
		public bool WebByPassDefaultPermissionCheck_37984 { get; set; }
		public IConfigReader ConfigReader { get; }

		public IocArgs(IConfigReader configReader)
		{
			FeatureToggle = configReader.AppConfig("FeatureToggle");
			ToggleMode = configReader.AppConfig("ToggleMode");
			TenantServer = configReader.AppConfig("TenantServer");
			ConfigServer = configReader.AppConfig("ConfigServer");
			ReportServer = configReader.AppConfig("ReportServer");
			MatrixWebSiteUrl = configReader.AppConfig("MatrixWebSiteUrl");
			DisableWebSocketCors = configReader.ReadValue("DisableWebSocketCors", false);
			BehaviorTestServer = configReader.ReadValue("BehaviorTestServer", false);
			AllEventPublishingsAsSync = configReader.ReadValue("AllEventPublishingsAsSync", false);
			OptimizeScheduleChangedEvents_DontUseFromWeb = configReader.ReadValue("OptimizeScheduleChangedEvents_DontUseFromWeb", false);
			ConfigReader = configReader;
			DataSourceConfigurationSetter = Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForDesktop();
			ImplementationTypeForCurrentUnitOfWork = typeof(CurrentUnitOfWork);
			MessageBrokerUrl = configReader.AppConfig("MessageBroker");
		}
	}
}