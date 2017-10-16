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

		private IEnumerable<RtaTracerConfig> traces()
		{
			return _config.ReadAll().ForEach(t =>
				write(new TracingLog {Tracing = t?.UserCode}, "Tracing", t.Tenant)
			);
		}

		public void ProcessReceived()
		{
			write2(new ProcessReceivedLog {RecievedAt = _now.UtcDateTime()}, "Data received at");
		}

		public void ProcessProcessing()
		{
			write2(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		}

		public void ProcessActivityCheck()
		{
			write2(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");
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
			write(trace, "Received");
			return trace;
		}

		public void InvalidStateCode(StateTraceLog trace) => write(trace, "Invalid state code");
		public void StateProcessing(StateTraceLog trace) => write(trace, "Processing", _dataSource.CurrentName());
		public void InvalidAuthenticationKey(StateTraceLog trace) => write(trace, "Invalid authentication key");
		public void InvalidSourceId(StateTraceLog trace) => write(trace, "Invalid source Id");
		public void InvalidUserCode(StateTraceLog trace) => write(trace, "Invalid user code");
		public void NoChange(StateTraceLog trace) => write(trace, "No change");

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events) =>
			new[] {"Processed"}
				.Concat(events.EmptyIfNull().Select(e => e.GetType().Name))
				.ForEach(m => write(trace, m));

		private bool shouldTrace(string userCode)
		{
			var tracedUserCode = traces().FirstOrDefault()?.UserCode;
			if (tracedUserCode == null)
				return false;
			return tracedUserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase);
		}

		private void write2<T>(T log, string message)
		{
			var traces = this.traces();
			var currentTenant = _dataSource.CurrentName() != null;
			var currentTenantTrace = traces.Where(x => x.Tenant == _dataSource.CurrentName());
			var tenants = currentTenant ? currentTenantTrace : traces;
			tenants.ForEach(t =>
				write(log, message, t.Tenant)
			);
		}

		private void write<T>(T log, string message, string tenant = null)
		{
			if (log == null)
				return;
			_writer.Write(new RtaTracerLog<T>
			{
				Log = log,
				Message = message,
				Process = _process,
				Tenant = tenant ?? _dataSource.CurrentName()
			});
		}
	}
}