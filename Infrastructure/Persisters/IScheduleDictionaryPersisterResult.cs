using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionaryPersisterResult {
		IEnumerable<IPersistableScheduleData> ModifiedEntities { get; set; }
		IEnumerable<IPersistableScheduleData> AddedEntities { get; set; }
		IEnumerable<IPersistableScheduleData> DeletedEntities { get; set; }
	}
}