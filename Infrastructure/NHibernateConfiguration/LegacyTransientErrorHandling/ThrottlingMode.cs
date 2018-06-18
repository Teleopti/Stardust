using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public enum ThrottlingMode
	{
		Unknown = -1,
		NoThrottling = 0,
		RejectUpdateInsert = 1,
		RejectAllWrites = 2,
		RejectAll = 3,
	}
}