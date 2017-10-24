using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
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
			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME")
						  ?? Environment.GetEnvironmentVariable("HOSTNAME");
			var processId = Process.GetCurrentProcess().Id.ToString();
			return $"{boxName}:{processId}";
		}

		public void RefreshTracers() =>
			_tracers = _config.ReadAll().Select(tracer =>
				{
					var config = makeTracerFromConfig(tracer);
					justWrite(new TracingLog {Tracing = config.User}, "Tracing", config.Tenant);
					return config;
				})
				.ToArray();

		[TenantScope, UnitOfWork]
		protected virtual tracer makeTracerFromConfig(RtaTracerConfig tracer)
		{
			_mapper.Refresh();
			var personIdsForUserCode = _mapper.PersonIdsFor(tracer.UserCode);
			return new tracer
			{
				Tenant = tracer.Tenant,
				UserCode = tracer.UserCode,
				User = $"{tracer.UserCode}: {string.Join(", ", personIdsForUserCode.Select(x => _persons.Get(x).Name))}",
				Persons = personIdsForUserCode
			};
		}

		public void FlushBuffer() => _writer.Flush();

		public void Trace(string usercode) => _config.UpdateForTenant(usercode);
		public void Stop() => _config.DeleteForTenant();
		public void Clear() => _writer.Clear();

		public void ProcessReceived(string method, int? count) => writeForCurrentOrConfigured(new ProcessReceivedLog
		{
			ReceivedAt = _now.UtcDateTime(),
			ReceivedBy = method,
			ReceivedCount = count ?? 0
		}, "Data received at");

		public void ProcessProcessing() => writeForCurrentOrConfigured(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		public void ProcessActivityCheck() => writeForCurrentOrConfigured(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");
		public void ProcessException(Exception exception) => writeForCurrentOrConfigured(new ProcessExceptionLog {Type = exception.GetType().Name}, exception.Message);

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace)
		{
			if (!enabled())
				return;
			traces.ForEach(trace);
		}

		public StateTraceLog StateReceived(string userCode, string stateCode) => startStateTrace(tracerFor(userCode), stateCode, "Received");
		public StateTraceLog ActivityCheck(Guid personId) => startStateTrace(tracerFor(personId), null, "Activity checked");
		public StateTraceLog SnapshotLogout(Guid personId, string stateCode) => null;

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
			public string UserCode;
			public string User;
			public IEnumerable<Guid> Persons;
		}

		private tracer tracerFor(string userCode)
		{
			if (!enabled())
				return null;
			return (
				from t in tracers()
				where t.Tenant == _dataSource.CurrentName() &&
					  t.UserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase)
				select t
			).FirstOrDefault();
		}

		private tracer tracerFor(Guid personId)
		{
			if (!enabled())
				return null;
			return (
				from t in tracers()
				where t.Tenant == _dataSource.CurrentName() &&
					  t.Persons.Any(p => p == personId)
				select t
			).FirstOrDefault();
		}

		private bool enabled() => tracers().Any();

		private IEnumerable<tracer> tracers()
		{
			if (_tracers == null)
				RefreshTracers();
			return _tracers;
		}

		private void writeForCurrentOrConfigured<T>(T log, string message)
		{
			if (!enabled())
				return;
			var allTracers = tracers();
			var hasCurrentTenant = _dataSource.CurrentName() != null;
			var currentTenantTracer = allTracers.Where(x => x.Tenant == _dataSource.CurrentName());
			var tracersToWrite = hasCurrentTenant ? currentTenantTracer : allTracers;
			tracersToWrite.ForEach(t =>
				justWrite(log, message, t.Tenant)
			);
		}

		private StateTraceLog startStateTrace(tracer tracer, string stateCode, string message)
		{
			if (tracer == null)
				return null;
			var trace = new StateTraceLog
			{
				Id = Guid.NewGuid(),
				User = tracer.UserCode,
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
				Log = log,
				Message = message,
				Process = _process,
				Tenant = tenant
			});
	}
}