using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Rta.TestApplication.TeleoptiRtaService;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public class TeleoptiRtaServiceProxy : TeleoptiRtaService.TeleoptiRtaService, IRtaDataHandlerClient
    {
        public int ProcessRtaData(string authenticationKey, string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
        {
            int result;
            bool resultSpecified;
            SaveExternalUserState(authenticationKey, logOn, stateCode, stateCode, true, true, (int)timeInState.TotalSeconds, true, timestamp,
                                     true, platformTypeId.ToString(), sourceId, batchId, true, isSnapshot, true, out result, out resultSpecified);
	        return result;
        }

		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			GetUpdatedScheduleChange(personId.ToString(), businessUnitId.ToString(), timestamp, true);
		}

        public bool IsAlive
        {
            get { return true; }
        }
    }
}
