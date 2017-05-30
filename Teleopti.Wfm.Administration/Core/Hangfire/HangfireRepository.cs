using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireRepository
	{
		public long CountActiveJobs(SqlConnection connection)
		{
			var selectCommandText = "SELECT COUNT(*) FROM HangFire.Job WITH (NOLOCK) WHERE StateName IN ('Enqueued', 'Processing')";
			long ret = 0;
			using (var selectCommand = new SqlCommand(selectCommandText, connection))
			using (var reader = selectCommand.ExecuteReader())
				if (reader.HasRows)
					if (reader.Read())
						ret = reader.GetInt32(0);

			return ret;
		}

		public long CountSucceededJobs(SqlConnection connection)
		{
			var selectCommandText = "SELECT COUNT(*) FROM HangFire.Job WITH (NOLOCK) WHERE StateName = 'Succeeded'";
			long ret = 0;
			using (var selectCommand = new SqlCommand(selectCommandText, connection))
			using (var reader = selectCommand.ExecuteReader())
				if (reader.HasRows)
					if (reader.Read())
						ret = reader.GetInt32(0);

			return ret;
		}

		public IEnumerable<EventCount> EventCounts(SqlConnection connection, string stateName)
		{
			var selectCommandText = "SELECT Arguments FROM HangFire.Job WITH (NOLOCK) WHERE StateName = '" + stateName + "'";
			var jobs = new List<Type>();
			using (var selectCommand = new SqlCommand(selectCommandText, connection))
			using (var reader = selectCommand.ExecuteReader())
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

		public IEnumerable<OldEvent> OldestEvents(SqlConnection connection)
		{
			var selectCommandText = @"
SELECT TOP 5
	Arguments,
	CreatedAt
FROM HangFire.Job WITH (NOLOCK)
WHERE StateName IN ('Enqueued', 'Processing')
ORDER BY CreatedAt ASC";
			var ret = new List<OldEvent>();
			using (var selectCommand = new SqlCommand(selectCommandText, connection))
			using (var reader = selectCommand.ExecuteReader())
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

			return ret;
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