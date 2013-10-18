using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class PersistConflict
	{
		public PersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			ClientVersion = clientVersion;
			DatabaseVersion = databaseVersion;
		}

		public DifferenceCollectionItem<IPersistableScheduleData> ClientVersion { get; private set; }
		public IPersistableScheduleData DatabaseVersion { get; private set; }
	}
}