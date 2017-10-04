using System.Collections.Generic;
using System.Linq;
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
			// Yeeeeeeeeeah, lets not care about db performance or GC with creating millions of classes (and not structs)
			var tracers = _reader.ReadOfType<DataRecievedAtLog>()
				.Select(x => new Tracer
				{
					Process = x.Process,
					DataReceivedAt = x.Thing?.DataRecievedAt?.ToString("T")
				})
				.Concat(_reader.ReadOfType<ActivityCheckAtLog>()
					.Select(x => new Tracer
					{
						Process = x.Process,
						ActivityCheckAt = x.Thing?.ActivityCheckAt?.ToString("T")
					}))
				.Concat(_reader.ReadOfType<TracingLog>()
					.Select(x => new Tracer
					{
						Process = x.Process,
						Tracing = x.Thing?.Tracing
					}))
				.GroupBy(x => x.Process)
				.Select(g => new Tracer
				{
					Process = g.Key,
					ActivityCheckAt = g.Max(a => a?.ActivityCheckAt),
					DataReceivedAt = g.Max(d => d?.DataReceivedAt),
					Tracing = g.Last(x => x != null).Tracing
				}).ToArray();

			return new RtaTracerViewModel
			{
				Tracers = tracers
			};

//			var recievedLogs =
//				from p in _reader.ReadOfType<DataRecievedAtLog>()
//				group p by p.Process
//				into g
//				select new Tracer
//				{
//					Process = g.Key,
//					DataReceivedAt = g.Max(d => d.Thing?.DataRecievedAt?.ToString("T"))
//				};
//
//			var recievedLogs2 =
//				from p in _reader.ReadOfType<ActivityCheckAtLog>()
//				group p by p.Process
//				into g
//				select new Tracer
//				{
//					Process = g.Key,
//					ActivityCheckAt = g.Max(d => d.Thing?.ActivityCheckAt?.ToString("T"))
//				};
//
//			var recievedLogs3 =
//				from p in _reader.ReadOfType<TracingLog>()
//				group p by p.Process
//				into g
//				select new Tracer
//				{
//					Process = g.Key,
//					Tracing = g.LastOrDefault()?.Thing?.Tracing
//				};
//
//			return new RtaTracerViewModel()
//			{
//				Tracers = recievedLogs.Concat(recievedLogs2).Concat(recievedLogs3).ToArray()
//			};
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