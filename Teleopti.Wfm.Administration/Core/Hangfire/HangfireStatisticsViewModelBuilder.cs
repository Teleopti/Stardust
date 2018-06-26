using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireStatisticsViewModelBuilder
	{
		private readonly HangfireRepository _hangfireRepository;

		public HangfireStatisticsViewModelBuilder(HangfireRepository hangfireRepository)
		{
			_hangfireRepository = hangfireRepository;
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
			return _hangfireRepository.PerformanceStatistics();			
		}
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

	public class OldEvent {
		public string Type { get; set; }
		public string CreatedAt { get; set; }
		public string Duration { get; set; }
	}
}