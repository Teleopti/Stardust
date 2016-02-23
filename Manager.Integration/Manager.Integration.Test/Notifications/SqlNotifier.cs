using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Notifications
{
    public class SqlNotifier : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SqlNotifier));

        public SqlNotifier(string connectionString)
        {
            ConnectionString = connectionString;
        }

		public string ConnectionString { get; set; }

        public ManualResetEventSlim NotifyWhenAllNodesAreUp { get; set; }

        private CancellationTokenSource NotifyWhenAllNodesAreUpTaskCancellationTokenSource { get; set; }

        private Task NotifyWhenNodesAreUpTask { get; set; }

        public Task CreateNotifyWhenNodesAreUpTask(int numberOfNodes,
                                                      CancellationTokenSource cancellationTokenSource,
                                                      Func<int,int,bool> validator)
        {
            if (numberOfNodes <= 0)
            {
                throw new ArgumentException("invalid number of nodes");
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException("cancellationTokenSource");
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
                            new SqlCommand("SELECT COUNT(*) FROM Stardust.WorkerNodes",
                                           sqlConnection))
                        {
							var rowCount = (int) command.ExecuteScalar();

                            if (validator(rowCount,nodes))
                            {
                                NotifyWhenAllNodesAreUp.Set();
                            }
                        }

                        sqlConnection.Close();
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
            },
                                                   NotifyWhenAllNodesAreUpTaskCancellationTokenSource.Token);

            return NotifyWhenNodesAreUpTask;
        }

        public void Dispose()
        {
            LogHelper.LogDebugWithLineNumber("Start dispose.",
                                             Logger);

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

            LogHelper.LogDebugWithLineNumber("Finished dispose.",
                                             Logger);
        }
    }
}