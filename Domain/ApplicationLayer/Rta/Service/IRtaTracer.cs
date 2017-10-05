using System;
using System.Collections.Generic;
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

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
		}

		public StateTraceLog StateReceived(string userCode, string stateCode)
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

	public class RtaTracerLog<T>
	{
		public DateTime Time;
		public string Process;
		public string Message;
		public T Log;
	}

	public class DataRecievedAtLog
	{
		public DateTime? DataRecievedAt;
	}

	public class ActivityCheckAtLog
	{
		public DateTime? ActivityCheckAt;
	}

	public class TracingLog
	{
		public string Tracing;
	}

	public class StateTraceLog
	{
		public Guid Id;
		public string User;
		public string StateCode;
	}
	
	public interface IRtaTracerReader
	{
		IEnumerable<RtaTracerLog<T>> ReadOfType<T>();
	}

	public interface IRtaTracerWriter
	{
		void Write(string message);
	}

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