using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;
		private readonly IRtaTracerConfigPersister _config;
		private readonly string _process;
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSource;

		public RtaTracer(IRtaTracerWriter writer, IRtaTracerConfigPersister config, INow now, ICurrentDataSource dataSource)
		{
			_writer = writer;
			_config = config;
			_now = now;
			_dataSource = dataSource;
			_process = ProcessName();
		}

		public static string ProcessName()
		{
			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME")
						  ?? Environment.GetEnvironmentVariable("HOSTNAME");
			var processId = Process.GetCurrentProcess().Id.ToString();
			return $"{boxName}:{processId}";
		}

		public void Trace(string usercode) => _config.UpdateForTenant(usercode);

		public void Stop() => _config.DeleteForTenant();

		public void ProcessReceived() => writeForCurrentOrConfigured(new ProcessReceivedLog {RecievedAt = _now.UtcDateTime()}, "Data received at");
		public void ProcessProcessing() => writeForCurrentOrConfigured(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		public void ProcessActivityCheck() => writeForCurrentOrConfigured(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace) => traces.ForEach(trace);

		public StateTraceLog StateReceived(string userCode, string stateCode)
		{
			var tracer = tracerFor(userCode);
			if (tracer == null)
				return null;
			var trace = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = userCode,
				StateCode = stateCode
			};
			justWrite(trace, "Received", tracer.Tenant);
			return trace;
		}

		public void InvalidStateCode(StateTraceLog trace) => writeIfTraced(trace, "Invalid state code");
		public void StateProcessing(StateTraceLog trace) => writeIfTraced(trace, "Processing");
		public void InvalidAuthenticationKey(StateTraceLog trace) => writeIfTraced(trace, "Invalid authentication key");
		public void InvalidSourceId(StateTraceLog trace) => writeIfTraced(trace, "Invalid source Id");
		public void InvalidUserCode(StateTraceLog trace) => writeIfTraced(trace, "Invalid user code");
		public void NoChange(StateTraceLog trace) => writeIfTraced(trace, "No change");

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events) =>
			new[] {"Processed"}
				.Concat(events.EmptyIfNull().Select(e => e.GetType().Name))
				.ForEach(m => writeIfTraced(trace, m));


		private IEnumerable<RtaTracerConfig> tracers() => _config.ReadAll().ForEach(t =>
			justWrite(new TracingLog {Tracing = t?.UserCode}, "Tracing", t.Tenant)
		);

		private RtaTracerConfig tracerFor(string userCode) => (
			from t in tracers()
			where t.Tenant == _dataSource.CurrentName() &&
				  t.UserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase)
			select t
		).FirstOrDefault();

		private void writeForCurrentOrConfigured<T>(T log, string message)
		{
			var allTracers = tracers();
			var hasCurrentTenant = _dataSource.CurrentName() != null;
			var currentTenantTracer = allTracers.Where(x => x.Tenant == _dataSource.CurrentName());
			var tracersToWrite = hasCurrentTenant ? currentTenantTracer : allTracers;
			tracersToWrite.ForEach(t =>
				justWrite(log, message, t.Tenant)
			);
		}

		private void writeIfTraced(StateTraceLog log, string message)
		{
			if (log == null)
				return;
			justWrite(log, message, _dataSource.CurrentName());
		}

		private void justWrite<T>(T log, string message, string tenant) =>
			_writer.Write(new RtaTracerLog<T>
			{
				Log = log,
				Message = message,
				Process = _process,
				Tenant = tenant
			});
	}
}