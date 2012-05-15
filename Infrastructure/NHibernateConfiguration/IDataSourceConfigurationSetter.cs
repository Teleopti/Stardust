using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IDataSourceConfigurationSetter
	{
		void AddDefaultSettingsTo(Configuration nhConfiguration);
	}
}