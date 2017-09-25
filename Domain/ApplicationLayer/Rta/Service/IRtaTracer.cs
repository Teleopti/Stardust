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

		public StateTraceInfo CreateTrace(Func<string> userCode, Func<string> stateCode)
		{
			return null;
		}

		public void StateProcessing(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}

		public void StateRecevied(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}

		public void InvalidAuthenticationKey(Func<IEnumerable<StateTraceInfo>> traces)
		{
		}
	}
	
	public interface IRtaTracer
	{
		void ProcessReceived();
		void ProcessProcessing();

		StateTraceInfo CreateTrace(Func<string> userCode, Func<string> stateCode);
		void StateProcessing(Func<IEnumerable<StateTraceInfo>> traces);
		void StateRecevied(Func<IEnumerable<StateTraceInfo>> traces);
		void InvalidAuthenticationKey(Func<IEnumerable<StateTraceInfo>> traces);
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