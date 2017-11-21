using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public static class NHibernateConfigurationExtensions
	{
		public static void SetPropertyIfNotAlreadySet(this Configuration nhConfiguration, string key, string value)
		{
			if (nhConfiguration.GetProperty(key) == null)
				nhConfiguration.SetProperty(key, value);
		}
	}
}