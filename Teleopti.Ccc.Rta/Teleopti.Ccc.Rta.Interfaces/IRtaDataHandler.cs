using System;

namespace Teleopti.Ccc.Rta.Interfaces
{
    public interface IRtaDataHandler
    {
        int ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot);
	    void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp);
        bool IsAlive { get; }
    }
}