using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
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
		private readonly IExternalLogonReader _externalLogons;
		private readonly IPersonRepository _persons;

		public RtaTracer(IRtaTracerWriter writer,
			IRtaTracerConfigPersister config,
			INow now,
			ICurrentDataSource dataSource,
			IExternalLogonReader externalLogons,
			IPersonRepository persons)
		{
			_writer = writer;
			_config = config;
			_now = now;
			_dataSource = dataSource;
			_externalLogons = externalLogons;
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

		public void Trace(string usercode) => _config.UpdateForTenant(usercode);
		public void Stop() => _config.DeleteForTenant();
		public void Clear() => _writer.Clear();

		public void ProcessReceived() => writeForCurrentOrConfigured(new ProcessReceivedLog {RecievedAt = _now.UtcDateTime()}, "Data received at");
		public void ProcessProcessing() => writeForCurrentOrConfigured(new ProcessProcessingLog {ProcessingAt = _now.UtcDateTime()}, "Processing");
		public void ProcessActivityCheck() => writeForCurrentOrConfigured(new ActivityCheckLog {ActivityCheckAt = _now.UtcDateTime()}, "Activity check at");

		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace) => traces.ForEach(trace);

		public StateTraceLog StateReceived(string userCode, string stateCode) => writeTraceStart(tracerFor(userCode), stateCode, "Received");
		public StateTraceLog ActivityCheck(Guid personId) => writeTraceStart(tracerFor(personId), null, "Activity checked");
		public StateTraceLog SnapshotLogout(Guid personId, string stateCode) => null;

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

		protected class config
		{
			public string Tenant;
			public string UserCode;
			public string User;
			public IEnumerable<Guid> Persons;
		}

		private IEnumerable<config> tracers() =>
			_config.ReadAll().Select(tracer =>
				{
					var config = GetConfigForTenant(tracer);
					justWrite(new TracingLog {Tracing = config.User}, "Tracing", config.Tenant);
					return config;
				})
				.ToArray();


		[TenantScope, UnitOfWork, ReadModelUnitOfWork]
		protected virtual config GetConfigForTenant(RtaTracerConfig tracer)
		{
			var userCodePersonIds = _externalLogons.Read();
			var persons = _persons.LoadAll();
			var personIdsForUserCode = userCodePersonIds
				.Where(x => tracer.UserCode == x.UserCode)
				.Select(x => x.PersonId)
				.ToArray();
			var user = tracer.UserCode;
			foreach (var personId in personIdsForUserCode)
			{
				user += persons.Single(x => x.Id.Value == personId).Name;
			}

			return new config
			{
				Tenant = tracer.Tenant,
				UserCode = tracer.UserCode,
				User = user,
				Persons = personIdsForUserCode
			};
		}


		private config tracerFor(string userCode) => (
			from t in tracers()
			where t.Tenant == _dataSource.CurrentName() &&
				  t.UserCode.Equals(userCode, StringComparison.InvariantCultureIgnoreCase)
			select t
		).FirstOrDefault();

		private config tracerFor(Guid personId) => (
			from t in tracers()
			where t.Tenant == _dataSource.CurrentName() &&
				  t.Persons.Any(p => p == personId)
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

		private StateTraceLog writeTraceStart(config tracer, string stateCode, string message)
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