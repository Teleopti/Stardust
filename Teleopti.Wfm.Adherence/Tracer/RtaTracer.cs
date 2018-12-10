using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Tracer
{
	public class RtaTracer : IRtaTracer
	{
		private readonly IRtaTracerWriter _writer;
		private readonly IRtaTracerConfigPersister _config;
		private readonly string _process;
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSource;
		private readonly ExternalLogonMapper _mapper;
		private readonly IPersonRepository _persons;

		private IEnumerable<tracer> _tracers;

		public RtaTracer(IRtaTracerWriter writer,
			IRtaTracerConfigPersister config,
			INow now,
			ICurrentDataSource dataSource,
			ExternalLogonMapper mapper,
			IPersonRepository persons)
		{
			_writer = writer;
			_config = config;
			_now = now;
			_dataSource = dataSource;
			_mapper = mapper;
			_persons = persons;
			_process = ProcessName();
		}

		public static string ProcessName()
		{
			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME") ??
						  Environment.GetEnvironmentVariable("HOSTNAME");
			var processId = Process.GetCurrentProcess().Id.ToString();
			return $"{boxName}:{processId}";
		}

		public void RefreshTracers() =>
			_tracers = _config.ReadAll()
				.Select(config =>
				{
					var tracer = makeTracerFromConfig(config);
					justWrite(new TracingLog {Tracing = tracer.User}, "Tracing", tracer.Tenant);
					return tracer;
				})
				.ToArray();

		[TenantScope, UnitOfWork]
		protected virtual tracer makeTracerFromConfig(RtaTracerConfig config)
		{
			_mapper.Refresh();
			string user = null;
			var personIdsForUserCode = Enumerable.Empty<Guid>();

			if (config.UserCode != null)
			{
				personIdsForUserCode = _mapper.PersonIdsFor(config.UserCode);
				user = $"{config.UserCode}: {string.Join(", ", personIdsForUserCode.Select(x => _persons.Get(x).Name))}";
			}

			return new tracer
			{
				Tenant = config.Tenant,
				UserCode = config.UserCode,
				User = user,
				Persons = personIdsForUserCode
			};
		}

		public void FlushBuffer() => _writer.Flush();

		public void PurgeLogs()
		{
			if (!enabled())
				return;
			_writer.Purge();
		}

		public void Trace(string usercode) => _config.UpdateForTenant(usercode);
		public void Stop() => _config.DeleteForTenant();
		public void Clear() => _writer.Clear();

		public void ProcessReceived(string method, int? count) => writeProcessTrace(new ProcessReceivedLog
		{
			At = _now.UtcDateTime(),
			By = method,
			Count = count ?? 0
		}, "Data received at");

		public void ProcessEnqueuing(int? count) => writeProcessTrace(new ProcessEnqueuingLog {At = _now.UtcDateTime(), Count = count ?? 0}, "Enqueuing");
		public void ProcessProcessing(int? count) => writeProcessTrace(new ProcessProcessingLog {At = _now.UtcDateTime(), Count = count ?? 0}, "Processing");
		public void ProcessActivityCheck() => writeProcessTrace(new ProcessActivityCheckLog {At = _now.UtcDateTime()}, "Activity check at");
		public void ProcessException(Exception exception) => writeProcessTrace(new ProcessExceptionLog {Type = exception.GetType().Name, Info = exception.ToString()}, exception.Message);

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
			if (!enabled())
				return;
			traces.ForEach(trace);
		}

		public StateTraceLog StateReceived(string userCode, string stateCode) => startStateTrace(tracerFor(userCode), stateCode, "Received");
		public StateTraceLog ActivityCheck(Guid personId) => startStateTrace(tracerFor(personId), null, "Activity checked");
		public StateTraceLog SnapshotLogout(Guid personId, string stateCode) => startStateTrace(tracerFor(personId), stateCode, "Snapshot logout");

		public void InvalidStateCode(StateTraceLog trace) => writeIfTraced(trace, "Invalid state code");
		public void StateProcessing(StateTraceLog trace) => writeIfTraced(trace, "Processing");
		public void InvalidSourceId(StateTraceLog trace) => writeIfTraced(trace, "Invalid source Id");
		public void InvalidUserCode(StateTraceLog trace) => writeIfTraced(trace, "Invalid user code");
		public void NoChange(StateTraceLog trace) => writeIfTraced(trace, "No change");

		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events) =>
			new[] {"Processed"}
				.Concat(events.EmptyIfNull().Select(e => e.GetType().Name))
				.ForEach(m => writeIfTraced(trace, m));

		protected class tracer
		{
			public string Tenant;
			public string User;
			public string UserCode;
			public IEnumerable<Guid> Persons;
		}

		private tracer tracerFor(string userCode)
		{
			if (!enabled())
				return null;

			var tenant = _dataSource.CurrentName();
			return tracers()
				.Where(x => x.UserCode != null)
				.FirstOrDefault(t =>
					t.Tenant == tenant &&
					t.UserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase)
				);
		}

		private tracer tracerFor(Guid personId)
		{
			if (!enabled())
				return null;

			var tenant = _dataSource.CurrentName();
			return tracers().FirstOrDefault(t =>
				t.Tenant == tenant && t.Persons.Any(p => p == personId)
			);
		}

		private bool enabled() => tracers().Any();

		private IEnumerable<tracer> tracers()
		{
			if (_tracers == null)
				RefreshTracers();
			return _tracers;
		}

		private void writeProcessTrace<T>(T log, string message)
		{
			if (!enabled())
				return;
			var tenant = _dataSource.CurrentName();
			var noTenant = tenant == null;
			var currentTenantEnabled = tracers().Any(x => x.Tenant == tenant);
			if (noTenant || currentTenantEnabled)
				justWrite(log, message, tenant);
		}

		private StateTraceLog startStateTrace(tracer tracer, string stateCode, string message)
		{
			if (tracer == null)
				return null;
			var trace = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = tracer.User,
				StateCode = stateCode
			};
			justWrite(trace, message, tracer.Tenant);
			return trace;
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
				Time = _now.UtcDateTime(),
				Log = log,
				Message = message,
				Process = _process,
				Tenant = tenant
			});
	}
}