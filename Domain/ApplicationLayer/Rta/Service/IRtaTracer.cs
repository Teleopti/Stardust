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

		public StateTraceInfo TraceState(string userCode)
		{
			return null;
		}

		public void StateReceived(StateTraceInfo trace, string stateCode)
		{
		}

		public void StateProcessing(IEnumerable<StateTraceInfo> traces)
		{
		}
		
		public void InvalidStateCode(StateTraceInfo trace)
		{
		}
		
		public void InvalidAuthenticationKey(IEnumerable<StateTraceInfo> traces)
		{
		}

		public void InvalidUserCode(StateTraceInfo trace)
		{
		}

		public void InvalidSourceId(IEnumerable<StateTraceInfo> traces)
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

		StateTraceInfo TraceState(string userCode);
		void StateReceived(StateTraceInfo trace, string stateCode);
		void InvalidStateCode(StateTraceInfo trace);
		
		void StateProcessing(IEnumerable<StateTraceInfo> traces);
		void InvalidAuthenticationKey(IEnumerable<StateTraceInfo> traces);
		void InvalidSourceId(IEnumerable<StateTraceInfo> traces);
		
		void InvalidUserCode(StateTraceInfo trace);
		void NoChange(StateTraceInfo trace);
		void StateProcessed(StateTraceInfo trace, IEnumerable<IEvent> events);
	}
	
//
//	public class RtaTracer : IRtaTracer
//	{
//		private readonly INow _now;
//		private readonly string _traceThis = null;
//
//		public RtaTracer(INow now)
//		{
//			_now = now;
//		}
//		
//		public void ProcessReceived()
//		{
//		}
//
//		public void ProcessProcessing()
//		{
//		}
//
//		public StateTraceInfo StateRecevied(Func<string> userCode, Func<string> stateCode)
//		{
//			if (stateCode.Invoke().Equals(_traceThis))
//				return new StateTraceInfo
//				{
//					Time = _now.UtcDateTime()
//				};
//			return null;
//		}
//
//	}

	public class StateTraceInfo
	{
		public DateTime Time;
		public Guid Id;
	}
}