using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class NoRtaTracer : IRtaTracer
	{
		public void ProcessReceived()
		{
		}

		public void ProcessProcessing()
		{
		}

		public void For(Func<IEnumerable<StateTraceInfo>> traces, Action<StateTraceInfo> trace)
		{
		}

		public StateTraceInfo StateReceived(string userCode, string stateCode)
		{
			return null;
		}

		public void InvalidStateCode(StateTraceInfo trace)
		{
		}

		public void StateProcessing(StateTraceInfo trace)
		{
		}

		public void InvalidAuthenticationKey(StateTraceInfo trace)
		{
		}

		public void InvalidSourceId(StateTraceInfo trace)
		{
		}

		public void InvalidUserCode(StateTraceInfo trace)
		{
		}

		public void NoChange(StateTraceInfo trace)
		{
		}

		public void StateProcessed(StateTraceInfo trace, IEnumerable<IEvent> events)
		{
		}
	}

	public interface IRtaTracer
	{
		void ProcessReceived();
		void ProcessProcessing();

		void For(Func<IEnumerable<StateTraceInfo>> traces, Action<StateTraceInfo> trace);

		StateTraceInfo StateReceived(string userCode, string stateCode);
		void InvalidStateCode(StateTraceInfo trace);

		void StateProcessing(StateTraceInfo trace);
		void InvalidAuthenticationKey(StateTraceInfo trace);
		void InvalidSourceId(StateTraceInfo trace);

		void InvalidUserCode(StateTraceInfo trace);
		void NoChange(StateTraceInfo trace);
		void StateProcessed(StateTraceInfo trace, IEnumerable<IEvent> events);
	}

	public class StateTraceInfo
	{
		public DateTime Time;
		public Guid Id;
	}
}