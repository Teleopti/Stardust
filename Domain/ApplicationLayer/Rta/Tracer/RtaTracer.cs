using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;

		public RtaTracer(IRtaTracerWriter writer)
		{
			_writer = writer;
		}

		public void ProcessReceived()
		{
			_writer.Write("ProcessReceived");
		}

		public void ProcessProcessing()
		{
			throw new NotImplementedException();
		}

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
			throw new NotImplementedException();
		}

		public StateTraceLog StateReceived(string userCode, string stateCode)
		{
			throw new NotImplementedException();
		}

		public void InvalidStateCode(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void StateProcessing(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidAuthenticationKey(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidSourceId(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidUserCode(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void NoChange(StateTraceLog trace)
		{
			throw new NotImplementedException();
		}

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events)
		{
			throw new NotImplementedException();
		}
	}
}