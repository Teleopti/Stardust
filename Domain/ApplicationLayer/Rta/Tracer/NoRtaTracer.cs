using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class NoRtaTracer : IRtaTracer
	{
		public void Trace(string usercode)
		{
		}

		public void Stop()
		{
		}

		public void Clear()
		{
		}

		public void ProcessReceived()
		{
		}

		public void ProcessProcessing()
		{
		}

		public void ProcessActivityCheck()
		{
		}

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
		}

		public StateTraceLog StateReceived(string userCode, string stateCode)
		{
			return null;
		}

		public StateTraceLog ActivityCheck(Guid personId)
		{
			return null;
		}

		public StateTraceLog SnapshotLogout(Guid personId, string stateCode)
		{
			return null;
		}

		public void InvalidStateCode(StateTraceLog trace)
		{
		}

		public void StateProcessing(StateTraceLog trace)
		{
		}

		public void InvalidAuthenticationKey(StateTraceLog trace)
		{
		}

		public void InvalidSourceId(StateTraceLog trace)
		{
		}

		public void InvalidUserCode(StateTraceLog trace)
		{
		}

		public void NoChange(StateTraceLog trace)
		{
		}

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events)
		{
		}
	}
}