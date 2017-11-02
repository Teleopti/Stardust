﻿using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	public static class StardustRepositoryTestHelper
	{
		public static void AddJobToQueue(Job job)
		{
			const string insertQueueCommandText = @"insert into [Stardust].[JobQueue] 
											(JobId, Name, Serialized, Type, CreatedBy, Created, LockTimeStamp, Policy) 
											Values (@jobId, 'Test', @serialized, @type , 'Test' , GETDATE() ,null ,null)";

			using (var connection = new SqlConnection(connectionString()))
			{
				connection.Open();
				using (var comm = new SqlCommand(insertQueueCommandText, connection))
				{
					comm.Parameters.AddWithValue("@jobId", job.JobId);
					comm.Parameters.AddWithValue("@serialized", job.Serialized);
					comm.Parameters.AddWithValue("@type", job.Type);

					comm.ExecuteNonQuery();
				}
			}
		}

		public static void AddJob(Job job)
		{
			const string insertJobCommandText = @"insert into [Stardust].[Job] 
											(JobId, Name, CreatedBy, Created, Started, Ended, Serialized, Type, SentToWorkerNodeUri, Result, Policy) 
											Values (@jobId, 'Test', 'Test', GETDATE(), GETDATE(), GETDATE(), @serialized, @type , '123456' , 'Result' ,null)";

			using (var connection = new SqlConnection(connectionString()))
			{
				connection.Open();
				using (var comm = new SqlCommand(insertJobCommandText, connection))
				{
					comm.Parameters.AddWithValue("@jobId", job.JobId);
					comm.Parameters.AddWithValue("@serialized", job.Serialized);
					comm.Parameters.AddWithValue("@type", job.Type);

					comm.ExecuteNonQuery();
				}
			}
		}

		public static void ClearQueue()
		{
			using (var connection = new SqlConnection(connectionString()))
			{
				connection.Open();
				using (var comm = new SqlCommand("truncate table Stardust.JobQueue", connection))
				{
					comm.ExecuteNonQuery();
				}
			}
		}

		public static void ClearJobs()
		{
			using (var connection = new SqlConnection(connectionString()))
			{
				connection.Open();
				using (var comm = new SqlCommand("delete from Stardust.Job", connection))
				{
					comm.ExecuteNonQuery();
				}
			}
		}

		private static string connectionString()
		{
			return ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString;
		}
	}
}