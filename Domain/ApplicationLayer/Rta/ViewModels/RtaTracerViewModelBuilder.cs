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
			var processes = tracings.Select(x => x.Key).Concat(
					dataReceivedAts.Select(x => x.Key)).Concat(
					activityCheckerAts.Select(x => x.Key))
				.Distinct();
			var tracers = from process in processes
				let tracing = tracings[process].OrderBy(x => x.Time).LastOrDefault()?.Log?.Tracing
				let dataReceivedAt = dataReceivedAts[process].Max(r => r.Log?.RecievedAt)?.ToString("T")
				let activityCheckAt = activityCheckerAts[process].Max(r => r.Log?.ActivityCheckAt)?.ToString("T")
				select new Tracer
				{
					Process = process,
					Tracing = tracing,
					DataReceivedAt = dataReceivedAt,
					ActivityCheckAt = activityCheckAt
				};

			var logs = _reader.ReadOfType<StateTraceLog>();
			var users = logs.Select(x => x.Log.User).Distinct();
			var tracedUsers =
					from user in users
					let traceIds = logs.Where(x => x.Log.User == user).Select(x => x.Log.Id).Distinct()
					let states =
						from s in traceIds
						let logsForTraceId = logs.Where(x => x.Log.Id == s)
						let traces = from t in logsForTraceId
							where !t.Message.IsNullOrEmpty()
							select $"{(t.Process.IsNullOrEmpty() ? "" : $"{t.Process} :")}{t.Message}"
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
		public string ActivityCheckAt;
		public string Tracing;
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