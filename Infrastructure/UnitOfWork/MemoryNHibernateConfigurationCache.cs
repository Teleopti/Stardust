using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	// This class is mainly used to not have to re-create the nhib config between each infra test
	public class MemoryNHibernateConfigurationCache
	{
		private static readonly Dictionary<string, Configuration> configurationCache = new Dictionary<string, Configuration>();
		private static readonly Dictionary<string, ISessionFactory> sessionFactoryCache = new Dictionary<string, ISessionFactory>();

		public Configuration GetConfiguration(IDictionary<string, string> settings, Configuration defaultValue = null)
		{
			var key = getConfigKey(settings);
			if (key == null) return defaultValue;
			return configurationCache.ContainsKey(key) ? configurationCache[key] : defaultValue;
		}

		public void StoreConfiguration(IDictionary<string, string> settings, Configuration configuration)
		{
			var key = getConfigKey(settings);
			if (key == null) return;
			configurationCache[key] = configuration;
		}

		public ISessionFactory GetSessionFactory(Configuration configuration, ISessionFactory defaultValue = null)
		{
			var key = getSessionFactoryKey(configuration);
			if (key == null) return defaultValue;
			return sessionFactoryCache.ContainsKey(key) ? sessionFactoryCache[key] : defaultValue;
		}

		public void StoreSessionFactory(Configuration configuration, ISessionFactory sessionFactory)
		{
			var key = getSessionFactoryKey(configuration);
			if (key == null) return;
			sessionFactoryCache[key] = sessionFactory;
		}

		public void Clear(IDictionary<string, string> settings)
		{
			var config = GetConfiguration(settings);
			if (config != null)
			{
				var session = GetSessionFactory(config);
				if (session != null)
					sessionFactoryCache[getSessionFactoryKey(config)] = null;
				configurationCache[getConfigKey(settings)] = null;
			}
		}

		private static string getConfigKey(IDictionary<string, string> settings)
		{
			return settings.ContainsKey(Environment.SessionFactoryName) ? settings[Environment.SessionFactoryName] : null;
		}

		private static string getSessionFactoryKey(Configuration configuration)
		{
			return configuration.Properties.ContainsKey(Environment.SessionFactoryName) ? configuration.Properties[Environment.SessionFactoryName] : null;
		}
	}
}