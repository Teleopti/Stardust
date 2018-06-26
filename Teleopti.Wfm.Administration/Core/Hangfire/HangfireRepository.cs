using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireRepository
	{
		private readonly string _connectionString;

		public HangfireRepository(IConfigReader config)
		{
			_connectionString = config.ConnectionString("Hangfire");
		}

		public long CountActiveJobs()
		{
			var selectCommandText = "SELECT COUNT(*) FROM HangFire.Job WITH (NOLOCK) WHERE StateName IN ('Enqueued', 'Processing')";
			long ret = 0;
			executeSql(selectCommandText, command =>
			{
				using (var reader = command.ExecuteReader())
					if (reader.HasRows)
						if (reader.Read())
							ret = reader.GetInt32(0);
			});
			return ret;
		}

		public long CountSucceededJobs()
		{
			var selectCommandText = "SELECT COUNT(*) FROM HangFire.Job WITH (NOLOCK) WHERE StateName = 'Succeeded'";
			long ret = 0;
			executeSql(selectCommandText, command =>
			{
				using (var reader = command.ExecuteReader())
					if (reader.HasRows)
						if (reader.Read())
							ret = reader.GetInt32(0);
			});
			return ret;
		}

		public IEnumerable<EventCount> EventCounts(string stateName)
		{
			var selectCommandText = "SELECT Arguments FROM HangFire.Job WITH (NOLOCK) WHERE StateName = '" + stateName + "'";
			var jobs = new List<Type>();

			executeSql(selectCommandText, command =>
			{
				using (var reader = command.ExecuteReader())
					if (reader.HasRows)
						while (reader.Read())
						{
							var data = reader.GetString(0);
							var arguments = JsonConvert.DeserializeObject<string[]>(data);
							string type; // We need to match the signatures of different legacy HangfireEventServer.Process() calls
							if (arguments.Length == 5)
								type = arguments[2].Replace("\"", "");
							else if (arguments.Length == 6)
								type = arguments[3].Replace("\"", "");
							else if (arguments.Length == 7)
								type = arguments[4].Replace("\"", "");
							else
								type = deserializeProperty("$type", arguments[1]);
							jobs.Add(Type.GetType(type));
						}
			});

			return jobs.Aggregate(new Dictionary<Type, EventCount>(), (counts, type) =>
				{
					if (counts.ContainsKey(type))
						counts[type].Count++;
					else
						counts[type] = new EventCount
						{
							Type = type.ToString(),
							Count = 1
						};

					return counts;
				})
				.Values;
		}

		public IEnumerable<OldEvent> OldestEvents()
		{
			var selectCommandText = @"
SELECT TOP 5
	Arguments,
	CreatedAt
FROM HangFire.Job WITH (NOLOCK)
WHERE StateName IN ('Enqueued', 'Processing')
ORDER BY CreatedAt ASC";
			var ret = new List<OldEvent>();
			executeSql(selectCommandText, command =>
			{
				using (var reader = command.ExecuteReader())
					if (reader.HasRows)
						while (reader.Read())
						{
							var createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
							var data = reader.GetString(reader.GetOrdinal("Arguments"));
							var arguments = JsonConvert.DeserializeObject<string[]>(data);
							var type = arguments[0];
							ret.Add(new OldEvent
							{
								Type = type,
								CreatedAt = createdAt.ToString("yyyy-MM-dd HH:mm:ss"),
								Duration = (DateTime.UtcNow - createdAt).ToString("g")
							});
						}
			});
			return ret;
		}

		public IEnumerable<JobStatistics> PerformanceStatistics()
		{
			var commandText = @"
SELECT 
	EventHandler,
	count(1) as TotalCount,
	sum(cast(Duration as bigint)) as TotalDuration,
	sum(cast(Duration as bigint))/count(cast(Duration as bigint)) as AverageDuration,
	Min(cast(Duration as bigint)) as MinDuration,
	Max(cast(Duration as bigint)) as MaxDuration
FROM (
		SELECT 
			substring(j.Arguments,5,charindex(' on',j.Arguments)-4) as EventHandler,
			JSON_VALUE(s.Data, '$.SucceededAt') as Date,
			JSON_VALUE(s.Data, '$.PerformanceDuration') as Duration
		FROM HangFire.Job j with(nolock)
				inner join hangfire.state s with(nolock) on s.id= j.stateid
		WHERE statename = 'Succeeded'
	) as d
GROUP BY EventHandler
ORDER BY TotalDuration  DESC";


			var performanceStatistics = new List<JobStatistics>();

			executeSql(commandText, cmd =>
			{
				using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						performanceStatistics.Add(new JobStatistics
						{
							Type = reader["EventHandler"].ToString(),
							Count = Convert.ToInt64(reader["TotalCount"]),
							TotalTime = Convert.ToInt64(reader["TotalDuration"]),
							AverageTime = Convert.ToInt64(reader["AverageDuration"]),
							MinTime = Convert.ToInt64(reader["MinDuration"]),
							MaxTime = Convert.ToInt64(reader["MaxDuration"])
						});
					}
				}
			});

			return performanceStatistics;
		}

		private void executeSql(string sql, Action<SqlCommand> action)
		{
			var connection = new SqlConnection(_connectionString);
			connection.Open();
			using (connection)
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				action(command);
			}
		}

		private static string deserializeProperty(string propertyName, string json)
		{
			using (var stringReader = new StringReader(json))
			using (var jsonReader = new JsonTextReader(stringReader))
				while (jsonReader.Read())
					if (jsonReader.TokenType == JsonToken.PropertyName && (string) jsonReader.Value == propertyName)
					{
						jsonReader.Read();
						var serializer = new JsonSerializer();
						return serializer.Deserialize<string>(jsonReader);
					}

			return null;
		}
	}
}