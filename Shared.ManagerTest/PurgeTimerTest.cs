using System;
using System.Configuration;
using System.Data.SqlClient;
using ManagerTest.Database;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Timers;

namespace ManagerTest
{
	[TestFixture]
	class PurgeTimerTest
	{
		private readonly string _managerConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;
		private readonly DatabaseHelper _databaseHelper = new DatabaseHelper();

		[SetUp]
		public void SetUp()
		{
			_databaseHelper.TryClearDatabase();

			// add a new job that should never be purged
			InsertJobRecords(1, 0); 
		}

		[Test]
		public void ShouldDeleteOldJobOnPurge()
		{
			InsertJobRecords(1, 2); //add a two hours old job
			JobPurgeTimer jobPurgeTimer = new JobPurgeTimer(new RetryPolicyProvider(), new ManagerConfiguration(_managerConnectionString, "Route", 10, 10, 1, 1, 1, 1));
			jobPurgeTimer.Purge();
			NumberOfJobsInDatabase().Should().Be.EqualTo(1);
			jobPurgeTimer.Dispose();
		}

		[Test]
		public void ShouldDeleteOldJobDetailOnPurge()
		{
			InsertJobRecords(1, 2); //add a two hours old job
			JobPurgeTimer jobPurgeTimer = new JobPurgeTimer(new RetryPolicyProvider(), new ManagerConfiguration(_managerConnectionString, "Route", 10, 10, 1, 1, 1, 1));
			jobPurgeTimer.Purge();
			NumberOfDetailsInDatabase().Should().Be.EqualTo(1);
			jobPurgeTimer.Dispose();
		}

		[Test]
		public void ShouldOnlyDeletebatchsizeNumberOfJobs()
		{
			InsertJobRecords(2, 2); //add a two hours old job
			JobPurgeTimer jobPurgeTimer = new JobPurgeTimer(new RetryPolicyProvider(), new ManagerConfiguration(_managerConnectionString, "Route", 10, 10, 1, 1, 1, 1));
			jobPurgeTimer.Purge();
			NumberOfJobsInDatabase().Should().Be.EqualTo(2);
			NumberOfDetailsInDatabase().Should().Be.EqualTo(2);
			jobPurgeTimer.Dispose();
		}


		private void InsertJobRecords(int numberOfJobs, int hoursAgoJobWasCreated)
		{
			using (var connection = new SqlConnection(_managerConnectionString))
			{
				string insertJobCommandText = @"INSERT INTO [Stardust].[Job] ([JobId]
								   ,[Name]
								   ,[Created]
								   ,[CreatedBy]
								   ,[Started]
								   ,[Ended]
								   ,[Serialized]
								   ,[Type]
								   ,[SentToWorkerNodeUri]
								   ,[Result])
							 VALUES
								   (@jobId
								   ,'Name'
								   ,DATEADD(HOUR, -@hours, GETDATE())
								   ,'CreatedBy'
								   ,NULL
								   ,NULL
								   ,'Serialized'
								   ,'Type'
								   ,'SentToWorkerNodeUri'
								   ,'Result')";

				string insertDetailCommandText = @"INSERT INTO [Stardust].[JobDetail]
																([JobId]
																,[Created]
																,[Detail])
															VALUES
																(@jobId
																, DATEADD(HOUR, -@hours, GETDATE())
																, 'Detail')";

				connection.Open();

				while (numberOfJobs > 0)
				{
					var jobId = Guid.NewGuid();
					using (var insertCommand = new SqlCommand(insertJobCommandText, connection))
					{
						insertCommand.Parameters.AddWithValue("@jobId", jobId);
						insertCommand.Parameters.AddWithValue("@hours", hoursAgoJobWasCreated);
						insertCommand.ExecuteNonQuery();
					}
					using (var insertCommand = new SqlCommand(insertDetailCommandText, connection))
					{
						insertCommand.Parameters.AddWithValue("@jobId", jobId);
						insertCommand.Parameters.AddWithValue("@hours", hoursAgoJobWasCreated);
						insertCommand.ExecuteNonQuery();
					}
					numberOfJobs--;
				}
			}
		}

		private int NumberOfJobsInDatabase()
		{
			var count = 0;
			using (var connection = new SqlConnection(_managerConnectionString))
			{
				string countJobsCommandText = @"Select count(*) from [Stardust].[Job]";

				connection.Open();
				using (var countCommand = new SqlCommand(countJobsCommandText, connection))
				{
					count = (int) countCommand.ExecuteScalar();
				}
			}
			return count;
		}

		private int NumberOfDetailsInDatabase()
		{
			var count = 0;
			using (var connection = new SqlConnection(_managerConnectionString))
			{
				string countJobsCommandText = @"Select count(*) from [Stardust].[JobDetail]";

				connection.Open();
				using (var countCommand = new SqlCommand(countJobsCommandText, connection))
				{
					count = (int)countCommand.ExecuteScalar();
				}
			}
			return count;
		}
	}
}
