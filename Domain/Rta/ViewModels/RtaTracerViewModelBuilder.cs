using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Rta.Tracer;

namespace Teleopti.Ccc.Domain.Rta.ViewModels
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
			var activityCheckLogs = _reader.ReadOfType<ActivityCheckLog>().ToLookup(x => x.Process);
			var exceptionLogs = _reader.ReadOfType<ProcessExceptionLog>().ToLookup(x => x.Process);
			var processLogs = tracingLogs.Select(x => x.Key).Concat(
					dataReceivedLogs.Select(x => x.Key)).Concat(
					activityCheckLogs.Select(x => x.Key)).Concat(
					exceptionLogs.Select(x => x.Key))
				.Distinct();

			var tracers = from process in processLogs
				let tracing = tracingLogs[process].OrderBy(x => x.Time).LastOrDefault()?.Log?.Tracing
				let dataReceived = dataReceivedLogs[process]
					.OrderByDescending(r => r.Log?.ReceivedAt)
					.Take(5)
					.Select(x =>
						new DataReceived
						{
							At = x.Log?.ReceivedAt?.ToString("HH:mm:ss"),
							By = x.Log?.ReceivedBy,
							Count = x.Log?.ReceivedCount ?? 0,
						})
				let activityCheckAt = activityCheckLogs[process].Max(r => r.Log?.ActivityCheckAt?.ToString("HH:mm:ss"))
				let exceptions = exceptionLogs[process]
					.OrderByDescending(x => x.Time)
					.Take(5)
					.Select(x => new TracedException
					{
						Exception = x.Log.Type,
						At = x.Time.ToString("yyyy-MM-dd HH:mm:ss"),
						Info = x.Log.Info
					})
				select new Tracer
				{
					Process = process,
					Tracing = tracing,
					DataReceived = dataReceived.ToArray(),
					ActivityCheckAt = activityCheckAt,
					Exceptions = exceptions
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
		public string ActivityCheckAt;
		public string Tracing;
		public IEnumerable<TracedException> Exceptions;
	}

	public class TracedException
	{
		public string Exception;
		public string At;
		public string Info;
	}

	public class DataReceived
	{
		public string At;
		public string By;
		public int Count;
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