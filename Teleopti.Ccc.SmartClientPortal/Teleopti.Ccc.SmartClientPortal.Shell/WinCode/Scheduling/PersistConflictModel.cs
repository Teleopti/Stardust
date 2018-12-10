using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class PersistConflictModel
    {
        public PersistConflictModel(IScheduleDictionary scheduleDictionary,
                                    IEnumerable<PersistConflict> persistConflicts,
									ICollection<IPersistableScheduleData> modifiedDataResult)
        {
            ScheduleDictionary = scheduleDictionary;
            PersistConflicts = persistConflicts;
            ModifiedDataResult = modifiedDataResult;
            Data = new List<PersistConflictData>();
        }

        public IScheduleDictionary ScheduleDictionary { get; private set; }
        public IEnumerable<PersistConflict> PersistConflicts { get; private set; }
		public ICollection<IPersistableScheduleData> ModifiedDataResult { get; private set; }
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