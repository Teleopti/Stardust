using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Manager.Integration.Test.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.Integration.Test.Notifications
{
	public class NodeStatusNotifier : IDisposable
	{
		public NodeStatusNotifier(string connectionString)
		{
			ConnectionString = connectionString;

			JobDefinitionStatusNotify = new ManualResetEventSlim();
		}

		public string ConnectionString { get; set; }

		public ManualResetEventSlim JobDefinitionStatusNotify { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		private Task JobDefinitionStatusNotifierTask { get; set; }

		public void Dispose()
		{
			this.Log().DebugWithLineNumber("Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			if (JobDefinitionStatusNotifierTask != null)
			{
				// Wait for task to finish.
				while (JobDefinitionStatusNotifierTask.Status == TaskStatus.Running)
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(500));
				}

				JobDefinitionStatusNotifierTask.Dispose();
			}

			this.Log().DebugWithLineNumber("Finished disposing.");
		}

		public void StartJobDefinitionStatusNotifier(Guid guid,
		                                             string status,
		                                             CancellationTokenSource cancellationTokenSource)
		{
			CancellationTokenSource = cancellationTokenSource;

			JobDefinitionStatusNotifierTask = Task.Factory.StartNew(() =>
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();

					var sqlCommand =
						@"SELECT COUNT(*) FROM Stardust.JobDefinitions 
                            WHERE Id = @Id AND Status = @Status";

					using (var command = new SqlCommand(sqlCommand,
					                                    connection))
					{
						command.Parameters.AddWithValue("@Id",
						                                guid);

						command.Parameters.AddWithValue("@Status",
						                                status);

						while (!cancellationTokenSource.IsCancellationRequested &&
						       !JobDefinitionStatusNotify.IsSet)
						{
							var numberOfRows = (int) command.ExecuteScalar();

							if (numberOfRows == 1)
							{
								JobDefinitionStatusNotify.Set();
							}

							Thread.Sleep(TimeSpan.FromSeconds(1));
						}
					}

					connection.Close();
				}

				if (cancellationTokenSource.IsCancellationRequested)
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			},
			                                                        cancellationTokenSource.Token);
		}
	}
}