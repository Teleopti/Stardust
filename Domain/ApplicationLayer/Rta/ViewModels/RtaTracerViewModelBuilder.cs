using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.FSharp.Reflection;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

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
			var dataReceivedAts = _reader.ReadOfType<DataRecievedAtLog>().ToLookup(x => x.Process);
			var activityCheckerAts = _reader.ReadOfType<ActivityCheckAtLog>().ToLookup(x => x.Process);
			var processes = tracings.Select(x => x.Key).Concat(
					dataReceivedAts.Select(x => x.Key)).Concat(
					activityCheckerAts.Select(x => x.Key))
				.Distinct();
			var tracers = from process in processes
				let tracing = tracings[process].OrderBy(x => x.Time).LastOrDefault()?.Log?.Tracing
				let dataReceivedAt = dataReceivedAts[process].Max(r => r.Log?.DataRecievedAt)?.ToString("T")
				let activityCheckAt = activityCheckerAts[process].Max(r => r.Log?.ActivityCheckAt)?.ToString("T")
				select new Tracer
				{
					Process = process,
					Tracing = tracing,
					DataReceivedAt = dataReceivedAt,
					ActivityCheckAt = activityCheckAt
				};

			/*
			all unique users
			all state codes (traceid) per user
			all messages per state code
			*/

			var receiveds = _reader.ReadOfType<StateTraceLog>();
			var messages = _reader.ReadOfType<StateTraceLog>().ToLookup(x => x.Log.User);
			var users = receiveds.Select(x => x.Log.User)
					.Concat(messages.Select(x => x.Key))
					.Distinct()
				;
			
			var tracedUsers = from user in users
					let receivedForUser = receiveds.Where(x => x.Log.User == user)
					let traceIds = receivedForUser.Select(x => x.Log.Id).Concat(messages[user].Select(x => x.Log.Id)).Distinct()
					let states = from t in traceIds
						select new TracedState
						{
							StateCode = receivedForUser.Where(x => x.Log.Id == t).Select(x => x.Log.StateCode).FirstOrDefault(),
							Traces = messages[user].Where(x => x.Message != null).Where(x => x.Log.Id == t).Select(x => x.Message).ToArray()
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