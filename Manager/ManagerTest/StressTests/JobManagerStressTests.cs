using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using NUnit.Framework;
using Stardust.Manager.Diagnostics;

namespace ManagerTest.StressTests
{
	[TestFixture, Ignore]
	public class JobManagerStressTests
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}


		private readonly object _lock = new object();

		public void InsertIntoJobDefintionsWorker(Guid id,
		                                          string name,
		                                          string type,
		                                          string serialized,
		                                          string createdby)
		{
			var insertCommandText =
				@"INSERT INTO [Stardust.JobDefinition]
							   ([JobId],
								[Name],
								[Type],
								[Serialized],
								[CreatedBy],
								[Created])
				 VALUES 
								(@JobId,
								 @Name,
								 @Type,
								@Serialized,
								@CreatedBy,
								@Created)";

			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				var sqlCommand = new SqlCommand(insertCommandText, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@JobId", id);
				sqlCommand.Parameters.AddWithValue("@Name", name);
				sqlCommand.Parameters.AddWithValue("@Type", type);
				sqlCommand.Parameters.AddWithValue("@Serialized", serialized);
				sqlCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
				sqlCommand.Parameters.AddWithValue("@CreatedBy", createdby);

				sqlCommand.ExecuteNonQuery();
			}
		}


		[Test]
		public void CheckAndAssignWithCodeLockTest()
		{
			var selectAllAliveWorkerNodesText =
				@"SELECT  [Id]
						  ,[Url]
						  ,[Heartbeat]
						  ,[Alive]
					  FROM [Stardust.WorkerNode]
					  where Alive =1";

			var selectTop1FromJobDefinitionsText =
				@"SELECT Top 1 [Id],[Name],[Created],[Type],[Serialized],[CreatedBy],[ProcessingTime]
				FROM [dbo].[Stardust.JobDefinition]";

			var insertIntoJobHistoryText = @"INSERT INTO [dbo].[Stardust.JobHistory]
								   ([Id]
								   ,[Name]
								   ,[Created]
								   ,[CreatedBy]
								   ,[Started]
								   ,[Ended]
								   ,[Serialized]
								   ,[Type]
								   ,[AssignedNode]
								   ,[Result])
							 VALUES
								   (@Id
								   ,@Name
								   ,@Created
								   ,@CreatedBy
								   ,@Started
								   ,@Ended
								   ,@Serialized
								   ,@Type
								   ,@AssignedNode
								   ,@Result)";


			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			lock (_lock)
			{
				using (var sqlConnection = new SqlConnection(connectionString))
				{
					sqlConnection.Open();

					var selectAllAliveWorkerNodesCommand =
						new SqlCommand(selectAllAliveWorkerNodesText, sqlConnection);

					string workerNode = null;
					var readerWorkerNodes = selectAllAliveWorkerNodesCommand.ExecuteReader();

					if (!readerWorkerNodes.HasRows)
					{
						readerWorkerNodes.Close();
						sqlConnection.Close();

						return;
					}

					var ordinalPosForUrl = readerWorkerNodes.GetOrdinal("Url");

					while (readerWorkerNodes.Read())
					{
						workerNode = readerWorkerNodes.GetString(ordinalPosForUrl);
					}

					readerWorkerNodes.Close();

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						var selectTop1FromJobDefinitionsCommand =
							new SqlCommand(selectTop1FromJobDefinitionsText, sqlConnection)
							{
								Transaction = sqlTransaction
							};

						var reader = selectTop1FromJobDefinitionsCommand.ExecuteReader();

						if (!reader.HasRows)
						{
							reader.Close();

							sqlConnection.Close();

							return;
						}

						reader.Read();

						var id = reader.GetGuid(reader.GetOrdinal("Id"));
						var name = reader.GetString(reader.GetOrdinal("Name"));
						var type = reader.GetString(reader.GetOrdinal("Type"));
						var serialized = reader.GetString(reader.GetOrdinal("Serialized"));

						var created = reader.GetDateTime(reader.GetOrdinal("Created"));
						var createdBy = reader.GetString(reader.GetOrdinal("CreatedBy"));

						reader.Close();

						// Simulate HTTP Post.

						Task<HttpResponseMessage> task = null;
						try
						{
							task = new Task<HttpResponseMessage>(() =>
							{
								Thread.Sleep(TimeSpan.FromMilliseconds(100));
								return new HttpResponseMessage(HttpStatusCode.OK);
							});

							task.Start();
							task.Wait();
						}
						catch (Exception)
						{
						}

						if (task != null && task.IsCompleted)
						{
							
						}

						var commandInsertIntoJobHistory = new SqlCommand(insertIntoJobHistoryText, sqlConnection)
						{
							Transaction = sqlTransaction
						};

						commandInsertIntoJobHistory.Parameters.AddWithValue("@Id", id);
						commandInsertIntoJobHistory.Parameters.AddWithValue("@Name", name);
						commandInsertIntoJobHistory.Parameters.AddWithValue("@Type", type);
						commandInsertIntoJobHistory.Parameters.AddWithValue("@Serialized", serialized);

						commandInsertIntoJobHistory.Parameters.AddWithValue("@Started", DateTime.UtcNow);

						commandInsertIntoJobHistory.Parameters.AddWithValue("@AssignedNode", workerNode);

						commandInsertIntoJobHistory.Parameters.AddWithValue("@CreatedBy", createdBy);
						commandInsertIntoJobHistory.Parameters.AddWithValue("@Created", created);

						commandInsertIntoJobHistory.Parameters.AddWithValue("@Ended", DBNull.Value);

						commandInsertIntoJobHistory.Parameters.AddWithValue("@Result", DBNull.Value);

						commandInsertIntoJobHistory.ExecuteNonQuery();


						var commandDeleteJobDefinition =
							new SqlCommand("DELETE FROM [dbo].[Stardust.JobDefinition] WHERE Id = @Id", sqlConnection)
							{
								Transaction = sqlTransaction
							};

						commandDeleteJobDefinition.Parameters.AddWithValue("@Id", id);

						commandDeleteJobDefinition.ExecuteNonQuery();

						sqlTransaction.Commit();
					}
				}
			}
		}

		[Test]
		public void CheckAndAssignWithSemafor()
		{
			//var selectAllAliveWorkerNodesText =
			//	@"SELECT  [Id]
			//			  ,[Url]
			//			  ,[Heartbeat]
			//			  ,[Alive]
			//		  FROM [ManagerDBOptimal].[dbo].[Stardust.WorkerNode]
			//		  where Alive =1";

			var selectTop1FromJobDefinitionsText =
				@"SELECT Top 1 [Id],[Name],[Created],[Type],[Serialized],[CreatedBy],[ProcessingTime]
				FROM [dbo].[Stardust.JobDefinition] WHERE ProcessingTime IS NULL";

			var updateJobDefintionCommandText =
				@"UPDATE [dbo].[Stardust.JobDefinition] SET ProcessingTime = @ProcessingTime
				WHERE Id  = @Id";


			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerDBOptimalConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var firstTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
				{
					var jobDefintionReaderCommand = new SqlCommand(selectTop1FromJobDefinitionsText, sqlConnection)
					{
						Transaction = firstTransaction
					};

					var jobdefintionReader = jobDefintionReaderCommand.ExecuteReader();

					if (!jobdefintionReader.HasRows)
					{
						jobdefintionReader.Close();
						sqlConnection.Close();

						return;
					}

					jobdefintionReader.Read();

					var id = jobdefintionReader.GetGuid(jobdefintionReader.GetOrdinal("Id"));

					jobdefintionReader.Close();

					var updateJobDefintionCommand = new SqlCommand(updateJobDefintionCommandText, sqlConnection)
					{
						Transaction = firstTransaction
					};

					updateJobDefintionCommand.Parameters.AddWithValue("@ProcessingTime", DateTime.UtcNow.AddHours(1));
					updateJobDefintionCommand.Parameters.AddWithValue("@Id", id);

					updateJobDefintionCommand.ExecuteNonQuery();

					firstTransaction.Commit();
				}
			}
		}

		[Test]
		public void InsertIntoJobDefintionsTest()
		{
			Parallel.For(1, 5000, i =>
			{
				InsertIntoJobDefintionsWorker(Guid.NewGuid(),
				                              "Job name " + i,
				                              "Type " + +i,
				                              "Serialized" + +i,
				                              "antons");
			});
		}

		[Test]
		public void InsertIntoJobHistoryDetailTest()
		{
			var insertCommandText = @"INSERT INTO [dbo].[Stardust.JobHistoryDetail]
								   ([Id]
								   ,[Created]
								   ,[Detail])
							 VALUES
								   (@Id
								   ,@Created
								   ,@Detail)";

			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var sqlTransaction = sqlConnection.BeginTransaction())
				{
					var sqlCommand = new SqlCommand(insertCommandText, sqlConnection);

					sqlCommand.Parameters.AddWithValue("@Id", Guid.NewGuid());
					sqlCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
					sqlCommand.Parameters.AddWithValue("@Detail", "Detail data");

					sqlCommand.Transaction = sqlTransaction;

					sqlCommand.ExecuteNonQuery();

					sqlTransaction.Commit();
				}
			}
		}

		[Test]
		public void InsertIntoJobHistoryTest()
		{
			var insertCommandText = @"INSERT INTO [Stardust].[Job]
								   ([JobId]
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
								   (@JobId
								   ,@Name
								   ,@Created
								   ,@CreatedBy
								   ,@Started
								   ,@Ended
								   ,@Serialized
								   ,@Type
								   ,@SentToWorkerNodeUri
								   ,@Result)";

			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var sqlTransaction = sqlConnection.BeginTransaction())
				{
					var sqlCommand = new SqlCommand(insertCommandText, sqlConnection);

					sqlCommand.Parameters.AddWithValue("@JobId", Guid.NewGuid());
					sqlCommand.Parameters.AddWithValue("@Name", "Name data");
					sqlCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
					sqlCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
					sqlCommand.Parameters.AddWithValue("@Ended", DateTime.UtcNow.AddDays(1));
					sqlCommand.Parameters.AddWithValue("@CreatedBy", "Created by data");
					sqlCommand.Parameters.AddWithValue("@Serialized", "Serialized data");
					sqlCommand.Parameters.AddWithValue("@Type", "Type data");
					sqlCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", "Assigned Node data");
					sqlCommand.Parameters.AddWithValue("@Result", "Result data");

					sqlCommand.Transaction = sqlTransaction;

					sqlCommand.ExecuteNonQuery();

					sqlTransaction.Commit();
				}
			}
		}

		[Test]
		public void InsertIntoWorkerNodes()
		{
			var insertCommandText = @"INSERT INTO [dbo].[Stardust.WorkerNode]
													   ([Id]
													   ,[Url]
													   ,[Heartbeat]
													   ,[Alive])
										VALUES
														(@Id
													   ,@Url
													   ,@Heartbeat
													   ,@Alive)";

			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerDBOptimalConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				var sqlCommand = new SqlCommand(insertCommandText, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@Id", Guid.NewGuid());
				sqlCommand.Parameters.AddWithValue("@Url", "http://localhost:5001");
				sqlCommand.Parameters.AddWithValue("@Heartbeat", DateTime.UtcNow);
				sqlCommand.Parameters.AddWithValue("@Alive", true);

				sqlCommand.ExecuteNonQuery();
			}
		}


		[Test]
		public void ReadFromJobDefinionsWithIsolationTest()
		{
			var selectCommandText =
				@"SELECT Top 1 [Id]
							   ,[Name]
							   ,[Created]
							   ,[Serialized]
							   ,[Type]
							   ,[UserName]
				FROM [dbo].[Stardust.JobDefinitions]";

			var connectionString =
				ConfigurationManager.ConnectionStrings["ManagerDBOptimalConnectionString"].ConnectionString;

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var sqlTransaction = sqlConnection.BeginTransaction())
				{
					var sqlCommand = new SqlCommand(selectCommandText, sqlConnection)
					{
						Transaction = sqlTransaction
					};


					var reader = sqlCommand.ExecuteReader();

					if (reader.HasRows)
					{
						while (reader.Read())
						{
						}
					}

					reader.Close();

					sqlTransaction.Commit();
				}
			}
		}


		[Test]
		public void StressTestCheckAndAssignWithCodeLock()
		{
			for (var i = 0; i < 10; i++)
			{
				InsertIntoWorkerNodes();
			}

			for (var i = 0; i < 100; i++)
			{
				InsertIntoJobDefintionsWorker(Guid.NewGuid(),
				                              "Name" + i,
				                              "Type" + i,
				                              "Serialized" + i,
				                              "anton");
			}

			var tasks = new List<Task>();

			for (var i = 0; i < 100; i++)
			{
				tasks.Add(new Task(CheckAndAssignWithCodeLockTest));
			}

			var managerStopWatch = new ManagerStopWatch();

			Parallel.ForEach(tasks, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, task => task.Start());

			Task.WaitAll(tasks.ToArray());

			Assert.IsTrue(tasks.All(task => task.IsCompleted),
			              "Elapsed time (seconds) to complete : " + managerStopWatch.Elapsed.Seconds);
		}

		[Test]
		public void StressTestCheckAndAssignWithSemafor()
		{
			for (var i = 0; i < 10; i++)
			{
				InsertIntoWorkerNodes();
			}

			for (var i = 0; i < 100; i++)
			{
				InsertIntoJobDefintionsWorker(Guid.NewGuid(),
				                              "Name" + i,
				                              "Type" + i,
				                              "Serialized" + i,
				                              "anton");
			}

			var tasks = new List<Task>();

			for (var i = 0; i < 100; i++)
			{
				tasks.Add(new Task(CheckAndAssignWithSemafor));
			}

			var managerStopWatch = new ManagerStopWatch();

			Parallel.ForEach(tasks, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, task => task.Start());

			Task.WaitAll(tasks.ToArray());

			Assert.IsTrue(tasks.All(task => task.IsCompleted),
			              "Elapsed time (seconds) to complete : " + managerStopWatch.Elapsed.Seconds);
		}


		[Test]
		public void StressTestNewStructure()
		{
			var managerStopWatch = new ManagerStopWatch();

			var tasks = new List<Task>();

			for (var i = 0; i < 10; i++)
			{
				InsertIntoWorkerNodes();
			}

			for (var i = 0; i < 1000; i++)
			{
				InsertIntoJobDefintionsWorker(Guid.NewGuid(),
				                              "Name" + i,
				                              "Type" + i,
				                              "Serialized" + i,
				                              "anton");
			}

			for (var i = 0; i < 1000; i++)
			{
				tasks.Add(new Task(CheckAndAssignWithCodeLockTest));
			}


			Parallel.ForEach(tasks, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, task => task.Start());

			Task.WaitAll(tasks.ToArray());

			var elapsedTime = managerStopWatch.Elapsed.Seconds;


			Assert.IsTrue(tasks.All(task => task.IsCompleted), "Elapsed time (seconds) to complete : " + elapsedTime);
		}

		[Test]
		public void StressTestNewStructure1()
		{
			var tasks = new List<Task>();

			for (var i = 0; i < 10; i++)
			{
				InsertIntoWorkerNodes();
			}

			var cancellationTokenSource = new CancellationTokenSource();

			var taskCreateJobDefintions = Task.Factory.StartNew(() =>
			{
				while (true)
				{
					var i = 0;

					i++;

					InsertIntoJobDefintionsWorker(Guid.NewGuid(),
					                              "Name" + i,
					                              "Type" + i,
					                              "Serialized" + i,
					                              "anton");

					if (cancellationTokenSource.IsCancellationRequested)
					{
						cancellationTokenSource.Token.ThrowIfCancellationRequested();
					}

					Thread.Sleep(TimeSpan.FromMilliseconds(200));
				}
			}, cancellationTokenSource.Token);


			var taskCheckAndAssign = Task.Factory.StartNew(() =>
			{
				while (true)
				{
					CheckAndAssignWithCodeLockTest();

					Thread.Sleep(TimeSpan.FromMilliseconds(100));

					if (cancellationTokenSource.IsCancellationRequested)
					{
						cancellationTokenSource.Token.ThrowIfCancellationRequested();
					}
				}
			}, cancellationTokenSource.Token);

			Task.WaitAll(taskCheckAndAssign, taskCreateJobDefintions);
		}
	}
}