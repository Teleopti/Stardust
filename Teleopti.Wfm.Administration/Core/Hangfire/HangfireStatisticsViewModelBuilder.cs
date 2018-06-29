using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireStatisticsViewModelBuilder
	{
		private readonly HangfireRepository _hangfireRepository;
		private readonly IJsonDeserializer _deserializer;

		public HangfireStatisticsViewModelBuilder(HangfireRepository hangfireRepository, IJsonDeserializer deserializer)
		{
			_hangfireRepository = hangfireRepository;
			_deserializer = deserializer;
		}

		public Statistics Build()
		{
			var totalEvents = _hangfireRepository.CountActiveJobs();
			var succeededEvents = _hangfireRepository.CountSucceededJobs();
			var oldestEvents = _hangfireRepository.OldestEvents();
			return new Statistics
			{
				Time = DateTime.UtcNow.ToString("HH:mm:ss"),
				TotalEventCount = totalEvents,
				SucceededEventCount = succeededEvents,
				OldestEvents = oldestEvents
			};
		}

		public IEnumerable<EventCount> BuildTypesOfEvents(string stateName)
		{
			return _hangfireRepository.EventCounts(stateName);
		}

		public IEnumerable<JobStatistics> BuildPerformanceStatistics()
		{
			return
				(
					from j in _hangfireRepository.SucceededJobs()
					let arguments = _deserializer.DeserializeObject<string[]>(j.Arguments)
					let data = _deserializer.DeserializeObject<dynamic>(j.Data)
					let name = _deserializer.DeserializeObject<string>(arguments.First())
					let type = name.Substring(0, name.IndexOf(" on "))
					let duration = data.PerformanceDuration
					let typeAndDuration = new {type, duration}
					group typeAndDuration by typeAndDuration.type
					into g
					let totalTime = g.Sum(x => x.duration)
					orderby totalTime descending
					select new JobStatistics
					{
						Type = g.Key,
						Count = g.Count(),
						TotalTime = totalTime,
						AverageTime = (long) Math.Floor(g.Average(x => x.duration)),
						MaxTime = g.Max(x => x.duration),
						MinTime = g.Min(x => x.duration)
					}
				)
				.ToArray();
		}

		public HangfireStatisticsViewModel BuildStatistics()
		{
			return new HangfireStatisticsViewModel
			{
				JobStatistics = BuildPerformanceStatistics()
			};
		}
	}

	public class HangfireStatisticsViewModel
	{
		public IEnumerable<JobStatistics> JobStatistics { get; set; }
	}

	public class JobStatistics
	{
		public string Type { get; set; }
		public long Count { get; set; }
		public long TotalTime { get; set; }
		public long AverageTime { get; set; }
		public long MaxTime { get; set; }
		public long MinTime { get; set; }
	}

	public class Job
	{
		public string Arguments { get; set; }
		public string Data { get; set; }
	}

	public class Statistics
	{
		public string Time { get; set; }
		public long TotalEventCount { get; set; }
		public long SucceededEventCount { get; set; }
		public IEnumerable<OldEvent> OldestEvents { get; set; }
	}

	public class EventCount
	{
		public string Type { get; set; }
		public int Count { get; set; }
	}

	public class OldEvent
	{
		public string Type { get; set; }
		public string CreatedAt { get; set; }
		public string Duration { get; set; }
	}
}