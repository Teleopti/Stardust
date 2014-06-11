using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.LogClientProxy.TeleoptiRtaService;

namespace Teleopti.Ccc.Rta.LogClientProxy
{
    public class TeleoptiRtaServiceProxy : TeleoptiRtaService.TeleoptiRtaService, IRtaDataHandlerClient
    {
        public int ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
        {
            int result;
            bool resultSpecified;
            SaveExternalUserState("!#¤atAbgT%", logOn, stateCode, stateCode, true, true, (int)timeInState.TotalSeconds, true, timestamp,
                                     true, platformTypeId.ToString(), sourceId, batchId, true, isSnapshot, true, out result, out resultSpecified);
	        return result;
        }

		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			GetUpdatedScheduleChange(personId.ToString(), businessUnitId.ToString(), timestamp, true);
		}

	    public WaitHandle ProcessRtaData(Guid platformTypeId, string sourceId, ICollection<ITeleoptiRtaState> rtaStates)
		{
			var listOfStates = new List<ExternalUserState>();
			foreach (var externalUserState in rtaStates)
			{
				listOfStates.Add(ProxyExtensions.ToDto(externalUserState));
			}
			int result;
			bool resultSpecified;
			SaveBatchExternalUserState("!#¤atAbgT%", platformTypeId.ToString(), sourceId, listOfStates.ToArray(), out result, out resultSpecified);
			return new AutoResetEvent(true);
		}

        public bool IsAlive
        {
            get { return true; }
        }

	    public void FlushCacheToDatabase()
	    {
		    throw new NotImplementedException();
	    }
    }

	public static class ProxyExtensions
	{
		public static ExternalUserState ToDto(ITeleoptiRtaState state)
		{
			return new ExternalUserState
			{
				BatchId = state.BatchId,
				BatchIdSpecified = true,
				IsLoggedOn = true,
				IsLoggedOnSpecified = true,
				IsSnapshot = state.IsSnapshot,
				IsSnapshotSpecified = true,
				SecondsInState = (int)state.TimeInState.TotalSeconds,
				SecondsInStateSpecified = true,
				StateCode = state.StateCode,
				StateDescription = state.StateCode,
				Timestamp = state.Timestamp,
				TimestampSpecified = true,
				UserCode = state.LogOn
			};
		}
	}
}
