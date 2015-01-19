using System.Configuration;
using Autofac;
using MbCache.Configuration;
using MbCache.ProxyImpl.LinFu;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommon
{
	public class IocArgs
	{
		public string FeatureToggle { get; set; }
		public string ToggleMode { get; set; }
		public string TennantServer { get; set; }


		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }
		public ILockObjectGenerator CacheLockObjectGenerator { get; set; }
		public IDataSourceConfigurationSetter DataSourceConfigurationSetter { get; set; }

		private CacheBuilder _cacheModule;

		public CacheBuilder CacheBuilder
		{
			get
			{
				return _cacheModule ?? (_cacheModule = new CacheBuilder(new LinFuProxyFactory())
					.SetCache(new InMemoryCache(20))
					.SetCacheKey(new TeleoptiCacheKey())
					.SetLockObjectGenerator(CacheLockObjectGenerator));
			}
		}

		public IocArgs()
		{
			FeatureToggle = ConfigurationManager.AppSettings["FeatureToggle"];
			ToggleMode = ConfigurationManager.AppSettings["ToggleMode"];
			TennantServer = ConfigurationManager.AppSettings["TennantServer"];
			DataSourceConfigurationSetter = Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForWeb();
		}
	}
}