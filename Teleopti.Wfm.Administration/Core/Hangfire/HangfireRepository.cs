using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

		public IEnumerable<Job> SucceededJobs()
		{
			var commandText = @"
SELECT 
	j.Arguments,
	s.Data
FROM Hangfire.Job j WITH (NOLOCK)
JOIN Hangfire.State s WITH (NOLOCK) ON 
	s.Id = j.StateId 
WHERE
	j.StateName = 'Succeeded'
";

			var jobs = new List<Job>();

			executeSql(commandText, cmd =>
			{
				using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						jobs.Add(new Job
						{
							Arguments = reader["Arguments"].ToString(),
							Data = reader["Data"].ToString(),
						});
					}
				}
			});

			return jobs;
		}
		
		
		public IEnumerable<Job> FailedJobs()
		{
			var commandText = @"
SELECT 
	Arguments
FROM Hangfire.Job WITH (NOLOCK)
WHERE
	StateName = 'Failed'
";

			var jobs = new List<Job>();

			executeSql(commandText, cmd =>
			{
				using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						jobs.Add(new Job
						{
							Arguments = reader["Arguments"].ToString(),
						});
					}
				}
			});

			return jobs;
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
	}
}