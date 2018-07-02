using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
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

		[RemoveMeWithToggle(Toggles.LevelUp_HangfireStatistics_76139_76373)]
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
		
		[RemoveMeWithToggle(Toggles.LevelUp_HangfireStatistics_76139_76373)]
		public IEnumerable<EventCount> BuildTypesOfEvents(string stateName)
		{
			return _hangfireRepository.EventCounts(stateName);
		}


		public HangfireStatisticsViewModel BuildStatistics()
		{
			return new HangfireStatisticsViewModel
			{
				JobPerformance = buildPerformanceStatistics(),
				JobFailures = buildJobFailures()					
			};
		}
		
		private IEnumerable<JobPerformance> buildPerformanceStatistics()
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
					select new JobPerformance
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

		private IEnumerable<JobFailure> buildJobFailures()
		{					
			return						
				(
					from j in _hangfireRepository.FailedJobs()
					let arguments = _deserializer.DeserializeObject<string[]>(j.Arguments)
					let name = _deserializer.DeserializeObject<string>(arguments.First())
					let type = name.Substring(0, name.IndexOf(" on "))
					group type by type
					into g
					let count = g.Count()
					orderby count descending
					select new JobFailure()
					{
						Type = g.Key,
						Count = count,
					}
				)
				.ToArray();			
		}
	}

	public class HangfireStatisticsViewModel
	{
		public IEnumerable<JobPerformance> JobPerformance { get; set; }
		public IEnumerable<JobFailure> JobFailures { get; set; }
	}

	public class JobFailure
	{
		public string Type { get; set; }
		public long Count { get; set; }
	}

	public class JobPerformance
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

	[RemoveMeWithToggle(Toggles.LevelUp_HangfireStatistics_76139_76373)]	
	public class Statistics
	{
		public string Time { get; set; }
		public long TotalEventCount { get; set; }
		public long SucceededEventCount { get; set; }
		public IEnumerable<OldEvent> OldestEvents { get; set; }
	}
	
	[RemoveMeWithToggle(Toggles.LevelUp_HangfireStatistics_76139_76373)]
	public class EventCount
	{
		public string Type { get; set; }
		public int Count { get; set; }
	}

	[RemoveMeWithToggle(Toggles.LevelUp_HangfireStatistics_76139_76373)]	
	public class OldEvent
	{
		public string Type { get; set; }
		public string CreatedAt { get; set; }
		public string Duration { get; set; }
	}
}