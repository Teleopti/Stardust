using System;
using System.Threading;

namespace Teleopti.Ccc.Rta.Interfaces
{
    public interface IRtaDataHandler
    {
        WaitHandle ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp,
                            Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot);

        bool IsAlive { get; }
    }
}