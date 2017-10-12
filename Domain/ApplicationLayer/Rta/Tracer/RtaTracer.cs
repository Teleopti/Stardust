using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly string _process;
		private readonly INow _now;
		private readonly WithReadModelUnitOfWork _readModelUnitOfWork;

		public RtaTracer(IRtaTracerWriter writer, IKeyValueStorePersister keyValues, INow now, WithReadModelUnitOfWork readModelUnitOfWork)
		{
			_writer = writer;
			_keyValues = keyValues;
			_now = now;
			_readModelUnitOfWork = readModelUnitOfWork;
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
			_readModelUnitOfWork.Do(() => _keyValues.Update("RtaTracerUserCode", usercode));
		}

		public void Stop()
		{
			_readModelUnitOfWork.Do(() => _keyValues.Delete("RtaTracerUserCode"));
		}

		public void ProcessReceived()
		{
			log(new ProcessReceivedLog {RecievedAt = _now.UtcDateTime()}, "Data received at");
		}

		public void ProcessProcessing()
		{
			log(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		}

		public void ProcessActivityCheck()
		{
			log(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");
		}

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
			traces.ForEach(trace);
		}

		public StateTraceLog StateReceived(string userCode, string stateCode)
		{
			var tracedUserCode = _readModelUnitOfWork.Get(() => _keyValues.Get("RtaTracerUserCode"));
			if (tracedUserCode == null || !tracedUserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase))
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

		public void InvalidStateCode(StateTraceLog trace)
		{
			log(trace, "Invalid state code");
		}

		public void StateProcessing(StateTraceLog trace)
		{
			log(trace, "Processing");
		}

		public void InvalidAuthenticationKey(StateTraceLog trace)
		{
			log(trace, "Invalid authentication key");
		}

		public void InvalidSourceId(StateTraceLog trace)
		{
			log(trace, "Invalid source Id");
		}

		public void InvalidUserCode(StateTraceLog trace)
		{
			log(trace, "Invalid user code");
		}

		public void NoChange(StateTraceLog trace)
		{
			log(trace, "No change");
		}

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events)
		{
			log(trace, "Processed");
			events.EmptyIfNull().ForEach(e => log(trace, e.GetType().Name));
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