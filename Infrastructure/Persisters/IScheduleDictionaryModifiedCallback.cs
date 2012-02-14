using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionaryModifiedCallback
	{
		void Callback(
			IScheduleDictionary scheduleDictionary,
			IEnumerable<IPersistableScheduleData> modifiedEntities,
			IEnumerable<IPersistableScheduleData> addedEntities,
			IEnumerable<IPersistableScheduleData> deletedEntities
			);

		void Callback(
			IScheduleRange scheduleRange,
			IEnumerable<IPersistableScheduleData> modifiedEntities,
			IEnumerable<IPersistableScheduleData> addedEntities,
			IEnumerable<IPersistableScheduleData> deletedEntities
			);
	}
}