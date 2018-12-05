using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Wfm.Adherence.Tracer
{
	public class RtaTracerViewModelBuilder
	{
		private readonly IRtaTracerReader _reader;

		public RtaTracerViewModelBuilder(IRtaTracerReader reader)
		{
			_reader = reader;
		}

		public RtaTracerViewModel Build()
		{
			var tracingLogs = _reader.ReadOfType<TracingLog>().ToLookup(x => x.Process);
			var dataReceivedLogs = _reader.ReadOfType<ProcessReceivedLog>().ToLookup(x => x.Process);
			var dataEnqueuingLogs = _reader.ReadOfType<ProcessEnqueuingLog>().ToLookup(x => x.Process);
			var dataProcessingLogs = _reader.ReadOfType<ProcessProcessingLog>().ToLookup(x => x.Process);
			var activityCheckLogs = _reader.ReadOfType<ProcessActivityCheckLog>().ToLookup(x => x.Process);
			var exceptionLogs = _reader.ReadOfType<ProcessExceptionLog>().ToLookup(x => x.Process);
			var processLogs = tracingLogs.Select(x => x.Key).Concat(
					dataReceivedLogs.Select(x => x.Key)).Concat(
					dataEnqueuingLogs.Select(x => x.Key)).Concat(
					dataProcessingLogs.Select(x => x.Key)).Concat(
					activityCheckLogs.Select(x => x.Key)).Concat(
					exceptionLogs.Select(x => x.Key))
				.Distinct();

			var tracers = from process in processLogs
				let tracing = tracingLogs[process].OrderBy(x => x.Time).LastOrDefault()
				let dataReceived = dataReceivedLogs[process]
					.OrderByDescending(r => r.Log?.At)
					.Take(5)
					.Select(x =>
						new DataReceived
						{
							At = x.Log?.At?.ToString("HH:mm:ss"),
							By = x.Log?.By,
							Count = x.Log?.Count ?? 0,
							Tenant = x.Tenant
						})
				let dataEnqueuings = dataEnqueuingLogs[process]
					.OrderByDescending(r => r.Log?.At)
					.Take(5)
					.Select(x =>
						new DataEnqueuing
						{
							At = x.Log?.At?.ToString("HH:mm:ss"),
							Count = x.Log?.Count ?? 0,
							Tenant = x.Tenant
						})
				let dataProcessings = dataProcessingLogs[process]
					.OrderByDescending(r => r.Log?.At)
					.Take(5)
					.Select(x =>
						new DataProcessing
						{
							At = x.Log?.At?.ToString("HH:mm:ss"),
							Count = x.Log?.Count ?? 0,
							Tenant = x.Tenant
						})
				let activityChecks = activityCheckLogs[process]
					.OrderByDescending(r => r.Log?.At)
					.Take(5)
					.Select(x =>
						new ActivityCheck
						{
							At = x.Log?.At?.ToString("HH:mm:ss"),
							Tenant = x.Tenant
						})
				let exceptions = exceptionLogs[process]
					.OrderByDescending(x => x.Time)
					.Take(5)
					.Select(x => new TracedException
					{
						Exception = x.Log?.Type,
						At = x.Time.ToString("yyyy-MM-dd HH:mm:ss"),
						Info = x.Log?.Info,
						Tenant = x.Tenant
					})
				select new Tracer
				{
					Process = process,
					Tracing = tracing?.Log?.Tracing,
					DataReceived = dataReceived.ToArray(),
					DataEnqueuing = dataEnqueuings.ToArray(),
					DataProcessing = dataProcessings.ToArray(),
					ActivityCheck = activityChecks.ToArray(),
					Exceptions = exceptions,
					Tenant = tracing?.Tenant
				};

			var logs = _reader.ReadOfType<StateTraceLog>().OrderByDescending(x => x.Time);
			var users = logs.Select(x => x.Log.User).Distinct();
			var tracedUsers =
					from user in users
					let traceIds = logs.Where(x => x.Log.User == user).Select(x => x.Log.Id).Distinct()
					let states =
						from s in traceIds
						let logsForTraceId = logs.Where(x => x.Log.Id == s)
						let traces = from t in logsForTraceId
							where !t.Message.IsNullOrEmpty()
							orderby t.Time
							select $"{(t.Process.IsNullOrEmpty() ? "" : $"{t.Process}: ")}{t.Message}"
						select new TracedState
						{
							StateCode = logsForTraceId.Select(x => x.Log.StateCode).FirstOrDefault(),
							Traces = traces.ToArray()
						}
					select new TracedUser
					{
						User = user,
						States = states.ToArray()
					}
				;

			return new RtaTracerViewModel
			{
				Tracers = tracers.ToArray(),
				TracedUsers = tracedUsers.ToArray()
			};
		}
	}

	public class RtaTracerViewModel
	{
		public IEnumerable<Tracer> Tracers;
		public IEnumerable<TracedUser> TracedUsers;
	}

	public class Tracer
	{
		public string Process;
		public IEnumerable<DataReceived> DataReceived;
		public IEnumerable<DataEnqueuing> DataEnqueuing;
		public IEnumerable<DataProcessing> DataProcessing;
		public IEnumerable<ActivityCheck> ActivityCheck;
		public string Tracing;
		public IEnumerable<TracedException> Exceptions;
		public string Tenant;
	}

	public class TracedException
	{
		public string Exception;
		public string At;
		public string Info;
		public string Tenant;
	}

	public class DataReceived
	{
		public string At;
		public string By;
		public int Count;
		public string Tenant;
	}

	public class DataEnqueuing
	{
		public string At;
		public int Count;
		public string Tenant;
	}

	public class DataProcessing
	{
		public string At;
		public int Count;
		public string Tenant;
	}

	public class ActivityCheck
	{
		public string At;
		public string Tenant;
	}

	public class TracedUser
	{
		public string User;
		public IEnumerable<TracedState> States;
	}

	public class TracedState
	{
		public string StateCode;
		public IEnumerable<string> Traces;
	}
}