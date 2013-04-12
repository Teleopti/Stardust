using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IDataSourceConfigurationSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nh")]
		void AddDefaultSettingsTo(Configuration nhConfiguration);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nh")]
		void AddApplicationNameToConnectionString(Configuration nhConfiguration);
	}
}