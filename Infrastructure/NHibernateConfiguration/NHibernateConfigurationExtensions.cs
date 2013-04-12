using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public static class NHibernateConfigurationExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nh")]
		public static void SetPropertyIfNotAlreadySet(this Configuration nhConfiguration, string key, string value)
		{
			if (nhConfiguration.GetProperty(key) == null)
				nhConfiguration.SetProperty(key, value);
		}
	}
}