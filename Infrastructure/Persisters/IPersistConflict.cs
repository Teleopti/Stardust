using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IPersistConflict
	{
		IPersistableScheduleData DatabaseVersion { get; }
		DifferenceCollectionItem<IPersistableScheduleData> ClientVersion { get; }
		void RemoveFromCollection();
	}
}