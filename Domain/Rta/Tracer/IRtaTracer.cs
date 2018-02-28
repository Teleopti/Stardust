using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public interface IRtaTracer
	{
		void RefreshTracers();
		void FlushBuffer();
		void PurgeLogs();
		

		void Trace(string usercode);
		void Stop();
		void Clear();


		void ProcessReceived(string method, int? count);
		void ProcessProcessing();
		void ProcessActivityCheck();
		void ProcessException(Exception exception);


		void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace);

		StateTraceLog StateReceived(string userCode, string stateCode);
		StateTraceLog ActivityCheck(Guid personId);
		StateTraceLog SnapshotLogout(Guid personId, string stateCode);

		void InvalidStateCode(StateTraceLog trace);

		void StateProcessing(StateTraceLog trace);
		void InvalidSourceId(StateTraceLog trace);

		void InvalidUserCode(StateTraceLog trace);
		void NoChange(StateTraceLog trace);
		void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events);
	}
}