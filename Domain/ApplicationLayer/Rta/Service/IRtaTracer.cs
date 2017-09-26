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

		public StateTraceInfo TraceState(Func<string> userCode)
		{
			return null;
		}

		public void StateReceived(Func<StateTraceInfo> trace, Func<string> stateCode)
		{
		}

		public void StateProcessing(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}
		
		public void InvalidStateCode(Func<StateTraceInfo> trace)
		{
		}
		
		public void InvalidAuthenticationKey(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}

		public void InvalidUserCode(Func<StateTraceInfo> trace)
		{
		}

		public void InvalidSourceId(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}

		public void NoChange(Func<StateTraceInfo> trace)
		{
		}

		public void StateProcessed(Func<StateTraceInfo> trace, Func<IEnumerable<IEvent>> events)
		{
		}
	}
	
	public interface IRtaTracer
	{
		void ProcessReceived();
		void ProcessProcessing();

		StateTraceInfo TraceState(Func<string> userCode);
		
		void StateReceived(Func<StateTraceInfo> trace, Func<string> stateCode);
		void StateProcessing(Func<IEnumerable<StateTraceInfo>> traces);
		void InvalidAuthenticationKey(Func<IEnumerable<StateTraceInfo>> traces);
		void InvalidSourceId(Func<IEnumerable<StateTraceInfo>> traces);
		
		void InvalidStateCode(Func<StateTraceInfo> trace);
		void InvalidUserCode(Func<StateTraceInfo> trace);
		void NoChange(Func<StateTraceInfo> trace);
		void StateProcessed(Func<StateTraceInfo> trace, Func<IEnumerable<IEvent>> events);
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