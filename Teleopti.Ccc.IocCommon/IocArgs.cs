using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Autofac;
using MbCache.Configuration;
using MbCache.ProxyImpl.LinFu;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon.Configuration;

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
		public bool PublishEventsToServiceBus { get; set; }
		public bool ThrottleMessages { get; set; }
		public int MessagesPerSecond { get; set; }
		public bool EnableNewResourceCalculation { get; set; }

		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }
		public IDataSourceConfigurationSetter DataSourceConfigurationSetter { get; set; }

		public bool ClearCache { get; set; }
		public List<Action<CacheBuilder>> CacheRegistrations = new List<Action<CacheBuilder>>(); 
		public void Cache(Action<CacheBuilder> builder)
		{
			CacheRegistrations.Add(builder);
		}

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
			PublishEventsToServiceBus = configReader.ReadValue("PublishEventsToServiceBus", true);
			DataSourceConfigurationSetter = Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForWeb();
			EnableNewResourceCalculation = configReader.ReadValue("EnableNewResourceCalculation", false);
		}

	}
}