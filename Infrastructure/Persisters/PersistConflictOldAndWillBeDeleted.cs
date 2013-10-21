using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class PersistConflictOldAndWillBeDeleted : IPersistConflict
	{
		public IPersistableScheduleData DatabaseVersion { get; set; }
		public DifferenceCollectionItem<IPersistableScheduleData> ClientVersion { get; set; }
		public void RemoveFromCollection() { }
	}
}