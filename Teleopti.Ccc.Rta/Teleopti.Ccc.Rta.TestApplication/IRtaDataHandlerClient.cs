using System;
using System.Collections.Generic;
using System.Threading;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public interface IRtaDataHandlerClient : IRtaDataHandler
    {
		WaitHandle ProcessRtaData(Guid platformTypeId, string sourceId, ICollection<ITeleoptiRtaState> rtaStates);

        string Url { get; set; }
        int Timeout { get; set; }
    }
}