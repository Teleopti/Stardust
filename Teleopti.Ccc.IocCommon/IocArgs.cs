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
		public string TenantServer { get; set; }
		public string ConfigServer { get; set; }
		public string ReportServer { get; set; }
		public string MatrixWebSiteUrl { get; set; }
		public bool ThrottleMessages { get; set; }
		public int MessagesPerSecond { get; set; }
		public bool OptimizeScheduleChangedEvents_DontUseFromWeb { get; set; }

		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }
		public IDataSourceConfigurationSetter DataSourceConfigurationSetter { get; set; }
		public Type ImplementationTypeForCurrentUnitOfWork { get; set; }

		public bool BehaviorTestServer { get; set; }
		public bool BehaviorTestClient { get; set; }

		public IocArgs(IConfigReader configReader)
		{
			FeatureToggle = configReader.AppConfig("FeatureToggle");
			ToggleMode = configReader.AppConfig("ToggleMode");
			TenantServer = configReader.AppConfig("TenantServer");
			ConfigServer = configReader.AppConfig("ConfigServer");
			ReportServer = configReader.AppConfig("ReportServer");
			MatrixWebSiteUrl = configReader.AppConfig("MatrixWebSiteUrl");
			ThrottleMessages = configReader.ReadValue("ThrottleMessages", true);
			MessagesPerSecond = configReader.ReadValue("MessagesPerSecond", 80);
			BehaviorTestServer = configReader.ReadValue("BehaviorTestServer", false);
			BehaviorTestClient = configReader.ReadValue("BehaviorTestClient", false);
			DataSourceConfigurationSetter = Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForDesktop();
			ImplementationTypeForCurrentUnitOfWork = typeof(CurrentUnitOfWork);
		}

	}
}