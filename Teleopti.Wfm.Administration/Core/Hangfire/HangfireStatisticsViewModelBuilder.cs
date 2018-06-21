﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireStatisticsViewModelBuilder
	{
		private readonly HangfireRepository _hangfireRepository;
		private readonly string _connectionString;

		public HangfireStatisticsViewModelBuilder(HangfireRepository hangfireRepository, string connectionString)
		{
			_hangfireRepository = hangfireRepository;
			_connectionString = connectionString;
		}
		
		public HangfireStatisticsViewModel Build()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var totalEvents = _hangfireRepository.CountActiveJobs(connection);
				var succeededEvents = _hangfireRepository.CountSucceededJobs(connection);
				var oldestEvents = _hangfireRepository.OldestEvents(connection);
				connection.Close();
				return new HangfireStatisticsViewModel
				{
					Time = DateTime.UtcNow.ToString("HH:mm:ss"),
					TotalEventCount = totalEvents,
					SucceededEventCount = succeededEvents,
					OldestEvents = oldestEvents
				};
			}
		}

		public IEnumerable<EventCount> BuildTypesOfEvents(string stateName)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var result = _hangfireRepository.EventCounts(connection, stateName);
				connection.Close();

				return result;
			}
		}

		public IEnumerable<HangfirePerformanceStatisticsViewModel> BuildPerformanceStatistics()
		{
			var jobStatistics = new List<HangfirePerformanceStatisticsViewModel>();

			using (var reader = _hangfireRepository.PerformanceStatistics(_connectionString))
			{
				while (reader.Read())
				{
					jobStatistics.Add(new HangfirePerformanceStatisticsViewModel
					{
						Type = reader["EventHandler"].ToString(),
						Count = Convert.ToInt64(reader["TotalExecutions"]),
						TotalExecutionTime = Convert.ToInt64(reader["TotalDuration"]),
						AverageExecutionTime = Convert.ToInt64(reader["AverageDuration"]),
						MinExecutionTime = Convert.ToInt64(reader["MinDuration"]),
						MaxExecutionTime = Convert.ToInt64(reader["MaxDuration"])
					});
				}
			}

			return jobStatistics;
		}
	}

	public class HangfirePerformanceStatisticsViewModel
	{
		public string Type { get; set; }
		public long Count { get; set; }
		public long TotalExecutionTime { get; set; }
		public long AverageExecutionTime { get; set; }
		public long MaxExecutionTime { get; set; }
		public long MinExecutionTime { get; set; }
	}

	public class HangfireStatisticsViewModel
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