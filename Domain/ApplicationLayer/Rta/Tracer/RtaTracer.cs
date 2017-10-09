using System;
using System.Collections.Generic;
using System.Diagnostics;
using NPOI.SS.Formula.Functions;
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
			_process = buildProcess();
		}

		private static string buildProcess()
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
			var log = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = userCode,
				StateCode = stateCode
			};
			_writer.Write(new RtaTracerLog<StateTraceLog>
			{
				Log = log,
				Message = "Received",
				Process = _process
			});
			return log;
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