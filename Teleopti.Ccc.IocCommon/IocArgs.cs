using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using Autofac;
using MbCache.Configuration;
using MbCache.ProxyImpl.LinFu;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;

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

		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }
		public ILockObjectGenerator CacheLockObjectGenerator { get; set; }
		public IDataSourceConfigurationSetter DataSourceConfigurationSetter { get; set; }
		public bool ClearCache { get; set; }

		private CacheBuilder _cacheModule;

		public CacheBuilder CacheBuilder
		{
			get
			{
				if (_cacheModule != null)
					return _cacheModule;
				if (ClearCache)
				{
					MemoryCache.Default
						.Select(x => x.Key)
						.ToList()
						.ForEach(x => MemoryCache.Default.Remove(x));
				}
				_cacheModule = new CacheBuilder(new LinFuProxyFactory())
					.SetCache(new InMemoryCache(20))
					.SetCacheKey(new TeleoptiCacheKey())
					.SetLockObjectGenerator(CacheLockObjectGenerator);
				return _cacheModule;
			}
		}

		public IocArgs(IAppConfigReader appConfigReader)
		{
			FeatureToggle = ConfigurationManager.AppSettings["FeatureToggle"];
			ToggleMode = ConfigurationManager.AppSettings["ToggleMode"];
			TenantServer = ConfigurationManager.AppSettings["TenantServer"];
			ConfigServer = ConfigurationManager.AppSettings["ConfigServer"];
			ReportServer = ConfigurationManager.AppSettings["ReportServer"];
			ReportServer = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			PublishEventsToServiceBus = readBoolAppSetting("PublishEventsToServiceBus", true);
			DataSourceConfigurationSetter = Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForWeb();
			ClearCache = false;
		}

		

		private bool readBoolAppSetting(string name, bool @default)
		{
			var value = ConfigurationManager.AppSettings[name];
			if (string.IsNullOrEmpty(value))
				return @default;
			bool result;
			return bool.TryParse(value, out result) ? result : @default;
		}
	}
}