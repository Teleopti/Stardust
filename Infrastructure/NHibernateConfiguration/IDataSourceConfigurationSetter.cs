using NHibernate.Cfg;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IDataSourceConfigurationSetter
	{
		[RemoveMeWithToggle("remove polly param", Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
		void AddDefaultSettingsTo(Configuration nhConfiguration, bool pollyResilientEnabled);
		void AddApplicationNameToConnectionString(Configuration nhConfiguration);
	}
}