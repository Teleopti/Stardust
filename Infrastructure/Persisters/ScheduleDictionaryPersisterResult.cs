using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDictionaryPersisterResult : IScheduleDictionaryPersisterResult 
	{
		public IEnumerable<IPersistableScheduleData> ModifiedEntities { get; set; }
		public IEnumerable<IPersistableScheduleData> AddedEntities { get; set; }
		public IEnumerable<IPersistableScheduleData> DeletedEntities { get; set; }
	}
}