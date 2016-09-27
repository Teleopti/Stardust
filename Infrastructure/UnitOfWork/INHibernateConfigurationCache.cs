using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface INHibernateConfigurationCache
	{
		Configuration GetOrDefault(IDictionary<string, string> settings);
		void Store(IDictionary<string, string> settings, Configuration configuration);

		ISessionFactory GetOrDefault(Configuration configuration);
		void StoreSessionFactory(Configuration configuration, ISessionFactory sessionFactory);
	}

	// This class is mainly used to not have to re-create the nhib config between each infra test
	public class MemoryNhibernateConfigurationCache : INHibernateConfigurationCache
	{
		private static readonly Dictionary<string, Configuration> configurationCache = new Dictionary<string, Configuration>();
		private static readonly Dictionary<string, ISessionFactory> sessionFactoryCache = new Dictionary<string, ISessionFactory>();

		public Configuration GetOrDefault(IDictionary<string, string> settings)
		{
			if (settings.ContainsKey(Environment.SessionFactoryName))
			{
				var key = settings[Environment.SessionFactoryName];
				if (configurationCache.ContainsKey(key))
					return configurationCache[key];
			}
			return null;
		}

		public void Store(IDictionary<string, string> settings, Configuration configuration)
		{
			if (!settings.ContainsKey(Environment.SessionFactoryName))
				return;
			var key = settings[Environment.SessionFactoryName];
			configurationCache[key] = configuration;
		}

		public ISessionFactory GetOrDefault(Configuration configuration)
		{
			if (configuration.Properties.ContainsKey(Environment.SessionFactoryName))
			{
				var key = configuration.Properties[Environment.SessionFactoryName];
				if (sessionFactoryCache.ContainsKey(key))
					return sessionFactoryCache[key];
			}
			return null;
		}

		public void StoreSessionFactory(Configuration configuration, ISessionFactory sessionFactory)
		{
			if (!configuration.Properties.ContainsKey(Environment.SessionFactoryName))
				return;
			var key = configuration.Properties[Environment.SessionFactoryName];
			sessionFactoryCache[key] = sessionFactory;
		}
	}

	public class NoNhibernateConfigurationCache : INHibernateConfigurationCache
	{
		public Configuration GetOrDefault(IDictionary<string, string> settings)
		{
			return null;
		}

		public void Store(IDictionary<string, string> settings, Configuration configuration)
		{
			
		}

		public ISessionFactory GetOrDefault(Configuration configuration)
		{
			return null;
		}

		public void StoreSessionFactory(Configuration configuration, ISessionFactory sessionFactory)
		{
			
		}
	}
}