using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public enum ThrottlingType
	{
		None,
		Soft,
		Hard,
		Unknown,
	}
}