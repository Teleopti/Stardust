using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
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
			var tracings = _reader.ReadOfType<TracingLog>().ToLookup(x => x.Process);
			var dataReceivedAts = _reader.ReadOfType<ProcessReceivedLog>().ToLookup(x => x.Process);
			var activityCheckerAts = _reader.ReadOfType<ActivityCheckLog>().ToLookup(x => x.Process);
			var exceptions = _reader.ReadOfType<ProcessExceptionLog>().ToLookup(x => x.Process);
			var processes = tracings.Select(x => x.Key).Concat(
					dataReceivedAts.Select(x => x.Key)).Concat(
					activityCheckerAts.Select(x => x.Key)).Concat(
					exceptions.Select(x => x.Key))
				.Distinct();

			var tracers = from process in processes
				let tracing = tracings[process].OrderBy(x => x.Time).LastOrDefault()?.Log?.Tracing
				let dataReceived = dataReceivedAts[process].OrderBy(r => r.Log?.ReceivedAt).LastOrDefault()?.Log
				let activityCheckAt = activityCheckerAts[process].Max(r => r.Log?.ActivityCheckAt)?.ToString("T")
				let exception = exceptions[process].OrderBy(x => x.Time).LastOrDefault()?.Log?.Type
				select new Tracer
				{
					Process = process,
					Tracing = tracing,
					DataReceivedAt = dataReceived?.ReceivedAt?.ToString("T"),
					DataReceivedBy = dataReceived?.ReceivedBy,
					DataReceivedCount = dataReceived?.ReceivedCount ?? 0,
					ActivityCheckAt = activityCheckAt,
					Exception = exception
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
		public string DataReceivedAt;
		public string DataReceivedBy;
		public int DataReceivedCount;
		public string ActivityCheckAt;
		public string Tracing;
		public string Exception;
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