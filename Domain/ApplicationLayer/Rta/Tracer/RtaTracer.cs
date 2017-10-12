using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly string _process;
		private readonly INow _now;

		public RtaTracer(IRtaTracerWriter writer, IKeyValueStorePersister keyValues, INow now)
		{
			_writer = writer;
			_keyValues = keyValues;
			_now = now;
			_process = ProcessName();
		}

		public static string ProcessName()
		{
			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME")
						  ?? Environment.GetEnvironmentVariable("HOSTNAME");
			var processId = Process.GetCurrentProcess().Id.ToString();
			return $"{boxName}:{processId}";
		}

		[ReadModelUnitOfWork]
		public virtual void Trace(string usercode) => _keyValues.Update("RtaTracerUserCode", usercode);

		[ReadModelUnitOfWork]
		public virtual void Stop() => _keyValues.Delete("RtaTracerUserCode");

		[ReadModelUnitOfWork]
		protected virtual string TracedUserCode()
		{
			var tracing = _keyValues.Get("RtaTracerUserCode");
			log(new TracingLog {Tracing = tracing}, "Tracing");
			return tracing;
		}

		public void ProcessReceived()
		{
			if (shouldTrace())
				log(new ProcessReceivedLog {RecievedAt = _now.UtcDateTime()}, "Data received at");
		}

		public void ProcessProcessing()
		{
			if (shouldTrace())
				log(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		}

		public void ProcessActivityCheck()
		{
			if (shouldTrace())
				log(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");
		}

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace) => traces.ForEach(trace);

		public StateTraceLog StateReceived(string userCode, string stateCode)
		{
			if (!shouldTrace(userCode))
				return null;
			var trace = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = userCode,
				StateCode = stateCode
			};
			log(trace, "Received");
			return trace;
		}

		public void InvalidStateCode(StateTraceLog trace) => log(trace, "Invalid state code");
		public void StateProcessing(StateTraceLog trace) => log(trace, "Processing");
		public void InvalidAuthenticationKey(StateTraceLog trace) => log(trace, "Invalid authentication key");
		public void InvalidSourceId(StateTraceLog trace) => log(trace, "Invalid source Id");
		public void InvalidUserCode(StateTraceLog trace) => log(trace, "Invalid user code");
		public void NoChange(StateTraceLog trace) => log(trace, "No change");

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events) =>
			new[] {"Processed"}
				.Concat(events.EmptyIfNull().Select(e => e.GetType().Name))
				.ForEach(m => log(trace, m));

		private bool shouldTrace() => TracedUserCode() != null;

		private bool shouldTrace(string userCode)
		{
			var tracedUserCode = TracedUserCode();
			if (tracedUserCode == null)
				return false;
			return tracedUserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase);
		}

		private void log<T>(T log, string message)
		{
			if (log == null)
				return;
			_writer.Write(new RtaTracerLog<T>
			{
				Log = log,
				Message = message,
				Process = _process
			});
		}
	}
}