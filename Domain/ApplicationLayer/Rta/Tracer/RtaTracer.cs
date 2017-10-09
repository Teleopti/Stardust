using System;
using System.Collections.Generic;
using System.Diagnostics;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly string _process;

		public RtaTracer(IRtaTracerWriter writer, IKeyValueStorePersister keyValues)
		{
			_writer = writer;
			_keyValues = keyValues;
			_process = ProcessName();
		}

		public static string ProcessName()
		{
			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME")
						  ?? Environment.GetEnvironmentVariable("HOSTNAME");
			var processId = Process.GetCurrentProcess().Id.ToString();
			return $"{boxName}:{processId}";
		}

		public void Trace(string usercode)
		{
			_keyValues.Update("RtaTracerUserCode", usercode);
		}

		public void Stop()
		{
			_keyValues.Delete("RtaTracerUserCode");
		}

		public void ProcessReceived()
		{
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
			var tracedUserCode = _keyValues.Get("RtaTracerUserCode");
			if (tracedUserCode == null || !tracedUserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase))
				return null;
			var trace = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = userCode,
				StateCode = stateCode
			};
			traceState(trace, "Received");
			return trace;
		}

		public void InvalidStateCode(StateTraceLog trace)
		{
			traceState(trace, "Invalid state code");
		}

		public void StateProcessing(StateTraceLog trace)
		{
			traceState(trace, "Processing");
		}

		public void InvalidAuthenticationKey(StateTraceLog trace)
		{
			traceState(trace, "Invalid authentication key");
		}

		public void InvalidSourceId(StateTraceLog trace)
		{
			traceState(trace, "Invalid source Id");
		}

		public void InvalidUserCode(StateTraceLog trace)
		{
			traceState(trace, "Invalid user code");
		}

		public void NoChange(StateTraceLog trace)
		{
			traceState(trace, "No change");
		}

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events)
		{
			traceState(trace, "Processed");
			events.EmptyIfNull().ForEach(e => traceState(trace, e.GetType().Name));
		}

		private void traceState(StateTraceLog trace, string message)
		{
			if (trace == null) return;
			_writer.Write(new RtaTracerLog<StateTraceLog>
			{
				Log = trace,
				Message = message,
				Process = _process
			});
		}
	}
}