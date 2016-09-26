using System.Collections.Generic;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface INHibernateConfigurationCache
	{
		Configuration GetOrDefault(IDictionary<string, string> settings);
		void Store(IDictionary<string, string> settings, Configuration configuration);
	}

	// This class is mainly used to not have to re-create the nhib config between each infra test
	public class MemoryNhibernateConfigurationCache : INHibernateConfigurationCache
	{
		private static readonly Dictionary<string, Configuration> configurationCache = new Dictionary<string, Configuration>();

		public Configuration GetOrDefault(IDictionary<string, string> settings)
		{
			if (settings.ContainsKey(Environment.SessionFactoryName))
			{
				var key = settings[Environment.SessionFactoryName];
				if (configurationCache.ContainsKey(key))
				{
					return configurationCache[key];
				}
			}
			return null;
		}

		public void Store(IDictionary<string, string> settings, Configuration configuration)
		{
			if (settings.ContainsKey(Environment.SessionFactoryName))
			{
				var key = settings[Environment.SessionFactoryName];
				configurationCache[key] = configuration;
			}
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
	}
}