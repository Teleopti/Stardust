using System;
using System.Collections.Generic;
using System.Threading;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public interface IRtaDataHandlerClient : IRtaDataHandler
    {
        string Url { get; set; }
        int Timeout { get; set; }
    }
}