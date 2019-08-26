using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Log4Net;

namespace Manager.Integration.Test.Database
{
	public class SqlNotifier : IDisposable
	{
		public SqlNotifier(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public string ConnectionString { get; set; }

		public ManualResetEventSlim NotifyWhenAllNodesAreUp { get; set; }

		private CancellationTokenSource NotifyWhenAllNodesAreUpTaskCancellationTokenSource { get; set; }

		private Task NotifyWhenNodesAreUpTask { get; set; }

		public void Dispose()
		{
			this.Log().DebugWithLineNumber("Start dispose.");

			if (NotifyWhenAllNodesAreUpTaskCancellationTokenSource != null &&
			    !NotifyWhenAllNodesAreUpTaskCancellationTokenSource.IsCancellationRequested)
			{
				NotifyWhenAllNodesAreUpTaskCancellationTokenSource.Cancel();
			}

			// Wait for task to complete.
			while (NotifyWhenNodesAreUpTask.Status == TaskStatus.Running)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(500));
			}

			NotifyWhenNodesAreUpTask.Dispose();

			this.Log().DebugWithLineNumber("Finished dispose.");
		}

		public Task CreateNotifyWhenNodesAreUpTask(int numberOfNodes,
		                                           CancellationTokenSource cancellationTokenSource)
		{
			if (numberOfNodes <= 0)
			{
				throw new ArgumentException("invalid number of nodes");
			}

			if (cancellationTokenSource == null)
			{
				throw new ArgumentNullException(nameof(cancellationTokenSource));
			}

			NotifyWhenAllNodesAreUp = new ManualResetEventSlim();

			NotifyWhenAllNodesAreUpTaskCancellationTokenSource = cancellationTokenSource;

            NotifyWhenNodesAreUpTask = new Task(() =>
                {
                    var nodes = numberOfNodes;

                    while (!cancellationTokenSource.IsCancellationRequested &&
                           !NotifyWhenAllNodesAreUp.IsSet)
                    {
                        using (var sqlConnection = new SqlConnection(ConnectionString))
                        {
                            sqlConnection.Open();

                            using (var command =
                                new SqlCommand("SELECT COUNT(*) FROM [Stardust].[WorkerNode] WHERE Alive=1",
                                    sqlConnection))
                            {
                                var rowCount = (int) command.ExecuteScalar();

                                if (rowCount == nodes)
                                {
                                    NotifyWhenAllNodesAreUp.Set();
                                }
                            }

                            sqlConnection.Close();
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }

                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                },
                NotifyWhenAllNodesAreUpTaskCancellationTokenSource.Token);

			return NotifyWhenNodesAreUpTask;
		}
	}
}