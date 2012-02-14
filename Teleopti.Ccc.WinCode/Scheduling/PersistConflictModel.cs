using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class PersistConflictModel
    {
        public PersistConflictModel(IScheduleDictionary scheduleDictionary,
                                    IEnumerable<IPersistConflict> persistConflicts,
                                    ICollection<IPersistableScheduleData> modifiedData)
        {
            ScheduleDictionary = scheduleDictionary;
            PersistConflicts = persistConflicts;
            ModifiedData = modifiedData;
            Data = new List<PersistConflictData>();
        }

        public IScheduleDictionary ScheduleDictionary { get; private set; }
        public IEnumerable<IPersistConflict> PersistConflicts { get; private set; }
        public ICollection<IPersistableScheduleData> ModifiedData { get; private set; }
        public IList<PersistConflictData> Data { get; private set; }
    }

    public class PersistConflictData
    {
        public string Name { get; set;}
        public DateOnly Date { get; set; }
        public string ConflictType { get; set; }
        public string LastModifiedName { get; set; }
    }

}