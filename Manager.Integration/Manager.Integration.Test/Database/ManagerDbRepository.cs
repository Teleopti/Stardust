using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using Manager.Integration.Test.Models;

namespace Manager.Integration.Test.Database
{
	public class ManagerDbRepository
	{
		private readonly ObservableCollection<JobDetail> _jobDetails = new ObservableCollection<JobDetail>();

		private readonly ObservableCollection<JobQueueItem> _jobQueueItems = new ObservableCollection<JobQueueItem>();

		private readonly ObservableCollection<Job> _jobs = new ObservableCollection<Job>();

		private readonly ObservableCollection<Logging> _loggings = new ObservableCollection<Logging>();

		private readonly ObservableCollection<PerformanceTest> _performanceTests = new ObservableCollection<PerformanceTest>();

		private readonly ObservableCollection<WorkerNode> _workerNodes = new ObservableCollection<WorkerNode>();

		public ManagerDbRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public string ConnectionString { get; private set; }
		
		public int JobQueueCount
		{
			get
			{
				try
				{
					using (var sqlConnection = new SqlConnection(ConnectionString))
					{
						sqlConnection.Open();

						var sqlCommand = new SqlCommand(@"SELECT COUNT(*)
													  FROM [Stardust].[JobQueue] WITH (NOLOCK)", sqlConnection);

						return (int)sqlCommand.ExecuteScalar();
					}

				}
				catch (Exception)
				{

				}

				return 0;
			}
		}

		public int JobCount
		{
			get
			{
				try
				{
					using (var sqlConnection = new SqlConnection(ConnectionString))
					{
						sqlConnection.Open();

						var sqlCommand = new SqlCommand(@"SELECT COUNT(*)
													  FROM [Stardust].[Job] WITH (NOLOCK)", sqlConnection);

						return (int)sqlCommand.ExecuteScalar();
					}

				}
				catch (Exception)
				{

				}

				return 0;
			}
		}
		
		public ObservableCollection<JobDetail> JobDetails
		{
			get
			{
				_jobDetails.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT [Id]
															  ,[JobId]
															  ,[Created]
															  ,[Detail]
														  FROM [Stardust].[JobDetail]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_jobDetails.Add(new JobDetail
							{
								Id = sqlDataReader.GetInt32(0),
								JobId = sqlDataReader.GetGuid(1),
								Created = sqlDataReader.GetDateTime(2),
								Detail = sqlDataReader.GetNullableString(3)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _jobDetails;
			}
		}

		public ObservableCollection<Job> Jobs
		{
			get
			{
				_jobs.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT [JobId]
														  ,[Name]
														  ,[CreatedBy]
														  ,[Created]
														  ,[Started]
														  ,[Ended]
														  ,[Serialized]
														  ,[Type]
														  ,[SentToWorkerNodeUri]
														  ,[Result]
													  FROM [Stardust].[Job]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_jobs.Add(new Job
							{
								JobId = sqlDataReader.GetGuid(0),
								Name = sqlDataReader.GetNullableString(1),
								CreatedBy = sqlDataReader.GetNullableString(2),
								Created = sqlDataReader.GetDateTime(3),
								Started = sqlDataReader.GetNullableDateTime(4),
								Ended = sqlDataReader.GetNullableDateTime(5),
								Serialized = sqlDataReader.GetNullableString(6),
								Type = sqlDataReader.GetNullableString(7),
								SentToWorkerNodeUri = sqlDataReader.GetNullableString(8),
								Result = sqlDataReader.GetNullableString(9)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _jobs;
			}
		}

		public ObservableCollection<JobQueueItem> JobQueueItems
		{
			get
			{
				_jobQueueItems.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT  [JobId]
															,[Name]
															,[Serialized]
															,[Type]
															,[CreatedBy]
															,[Created]
														FROM [Stardust].[JobQueue]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_jobQueueItems.Add(new JobQueueItem
							{
								JobId = sqlDataReader.GetGuid(0),
								Name = sqlDataReader.GetNullableString(1),
								Serialized = sqlDataReader.GetNullableString(2),
								Type = sqlDataReader.GetNullableString(3),
								CreatedBy = sqlDataReader.GetNullableString(4),
								Created = sqlDataReader.GetDateTime(5)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _jobQueueItems;
			}
		}

		public ObservableCollection<WorkerNode> WorkerNodes
		{
			get
			{
				_workerNodes.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT  [Id]
															  ,[Url]
															  ,[Heartbeat]
															  ,[Alive]
														  FROM [Stardust].[WorkerNode]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_workerNodes.Add(new WorkerNode
							{
								Id = sqlDataReader.GetGuid(0),
								Url = new Uri(sqlDataReader.GetString(1)),
								Heartbeat = sqlDataReader.GetDateTime(2),
								Alive = sqlDataReader.GetBoolean(3)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _workerNodes;
			}
		}


		public ObservableCollection<PerformanceTest> PerformanceTests
		{
			get
			{
				_performanceTests.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT [Id]
															  ,[Name]
															  ,[Description]
															  ,[Started]
															  ,[Ended]
															  ,[ElapsedInSeconds]
															  ,[ElapsedInMinutes]
														  FROM [Stardust].[PerformanceTest]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_performanceTests.Add(new PerformanceTest
							{
								Id = sqlDataReader.GetInt32(0),
								Name = sqlDataReader.GetNullableString(1),
								Description = sqlDataReader.GetNullableString(2),
								Started = sqlDataReader.GetDateTime(3),
								Ended = sqlDataReader.GetDateTime(4),
								ElapsedInSeconds = sqlDataReader.GetDouble(5),
								ElapsedInMinutes = sqlDataReader.GetDouble(6)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _performanceTests;
			}
		}

		public ObservableCollection<Logging> Loggings
		{
			get
			{
				_loggings.Clear();

				using (var sqlConnection = new SqlConnection(ConnectionString))
				{
					sqlConnection.Open();

					var sqlCommand = new SqlCommand(@"SELECT [Id]
															  ,[Date]
															  ,[Thread]
															  ,[Level]
															  ,[Logger]
															  ,[Message]
															  ,[Exception]
														  FROM [Stardust].[Logging]", sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReader();

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							_loggings.Add(new Logging
							{
								Id = sqlDataReader.GetInt32(0),
								Date = sqlDataReader.GetDateTime(1),
								Thread = sqlDataReader.GetNullableString(2),
								Level = sqlDataReader.GetNullableString(3),
								Logger = sqlDataReader.GetNullableString(4),
								Message = sqlDataReader.GetNullableString(5),
								Exception = sqlDataReader.GetNullableString(6)
							});
						}
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();
				}

				return _loggings;
			}
		}

		public void TruncateLoggingTable()
		{
			using (var sqlConnection = new SqlConnection(ConnectionString))
			{
				sqlConnection.Open();

				var sqlCommand =
					new SqlCommand(@"TRUNCATE TABLE [Stardust].[Logging]", sqlConnection);

				sqlCommand.ExecuteNonQuery();
			}
		}

		public void TruncateWorkerNodeTable()
		{
			using (var sqlConnection = new SqlConnection(ConnectionString))
			{
				sqlConnection.Open();

				var sqlCommand =
					new SqlCommand(@"TRUNCATE TABLE [Stardust].[WorkerNode]", sqlConnection);

				sqlCommand.ExecuteNonQuery();
			}
		}

		public void TruncateJobQueueTable()
		{
			using (var sqlConnection = new SqlConnection(ConnectionString))
			{
				sqlConnection.Open();

				var sqlCommand =
					new SqlCommand(@"TRUNCATE TABLE [Stardust].[JobQueue]", sqlConnection);

				sqlCommand.ExecuteNonQuery();
			}
		}

		public void TruncateJobDetailTable()
		{
			using (var sqlConnection = new SqlConnection(ConnectionString))
			{
				sqlConnection.Open();

				var sqlCommand =
					new SqlCommand(@"TRUNCATE TABLE [Stardust].[JobDetail]", sqlConnection);

				sqlCommand.ExecuteNonQuery();
			}
		}

		public void TruncateJobTable()
		{
			using (var sqlConnection = new SqlConnection(ConnectionString))
			{
				sqlConnection.Open();

				var sqlCommand =
					new SqlCommand(@"TRUNCATE TABLE [Stardust].[Job]", sqlConnection);

				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}