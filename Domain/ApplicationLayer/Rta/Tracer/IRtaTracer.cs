using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public interface IRtaTracer
	{
		void ProcessReceived();
		void ProcessProcessing();

		void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace);

		StateTraceLog StateReceived(string userCode, string stateCode);
		void InvalidStateCode(StateTraceLog trace);

		void StateProcessing(StateTraceLog trace);
		void InvalidAuthenticationKey(StateTraceLog trace);
		void InvalidSourceId(StateTraceLog trace);

		void InvalidUserCode(StateTraceLog trace);
		void NoChange(StateTraceLog trace);
		void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events);
	}
}