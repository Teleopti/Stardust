using System;
using System.Collections.Generic;
using log4net;
using log4net.Repository.Hierarchy;
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

		public void For(IEnumerable<StateTraceInfo> traces, Action<StateTraceInfo> trace)
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

		void For(IEnumerable<StateTraceInfo> traces, Action<StateTraceInfo> trace);

		StateTraceInfo StateReceived(string userCode, string stateCode);
		void InvalidStateCode(StateTraceInfo trace);

		void StateProcessing(StateTraceInfo trace);
		void InvalidAuthenticationKey(StateTraceInfo trace);
		void InvalidSourceId(StateTraceInfo trace);

		void InvalidUserCode(StateTraceInfo trace);
		void NoChange(StateTraceInfo trace);
		void StateProcessed(StateTraceInfo trace, IEnumerable<IEvent> events);
	}
	
	public class RtaTraceLog
	{
		public string Message { get; set; }
		public DateTime Time { get;  set; }
	}

	public interface IRtaTracerReader
	{
		IEnumerable<RtaTraceLog> Read();
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

		public void For(IEnumerable<StateTraceInfo> traces, Action<StateTraceInfo> trace)
		{
			throw new NotImplementedException();
		}

		public StateTraceInfo StateReceived(string userCode, string stateCode)
		{
			throw new NotImplementedException();
		}

		public void InvalidStateCode(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void StateProcessing(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidAuthenticationKey(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidSourceId(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void InvalidUserCode(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void NoChange(StateTraceInfo trace)
		{
			throw new NotImplementedException();
		}

		public void StateProcessed(StateTraceInfo trace, IEnumerable<IEvent> events)
		{
			throw new NotImplementedException();
		}

	}

	public class StateTraceInfo
	{
		public Guid Id;
	}
}