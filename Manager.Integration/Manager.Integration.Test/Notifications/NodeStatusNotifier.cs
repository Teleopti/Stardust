using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Notifications
{
	public class NodeStatusNotifier : IDisposable
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeStatusNotifier));

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
			LogHelper.LogDebugWithLineNumber("Start disposing.",
			                                 Logger);

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

			LogHelper.LogDebugWithLineNumber("Finished disposing.",
			                                 Logger);
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