using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public enum ThrottledResourceType
	{
		Unknown = -1,
		PhysicalDatabaseSpace = 0,
		PhysicalLogSpace = 1,
		LogWriteIoDelay = 2,
		DataReadIoDelay = 3,
		Cpu = 4,
		DatabaseSize = 5,
		Internal = 6,
		WorkerThreads = 7,
	}
}