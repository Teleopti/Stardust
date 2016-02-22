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

        public string ConnectionString { get; set; }

        public SqlNotifier(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ManualResetEventSlim NotifyWhenAllNodesAreUp { get; set; }

        private CancellationTokenSource NotifyWhenAllNodesAreUpTaskCancellationTokenSource { get; set; }

        private Task NotifyWhenAllNodesAreUpTask { get; set; }

        public Task CreateNotifyWhenAllNodesAreUpTask(int numberOfNodes,
                                                      CancellationTokenSource cancellationTokenSource)
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

            NotifyWhenAllNodesAreUpTask = new Task(() =>
            {
                int nodes = numberOfNodes;

                while (!cancellationTokenSource.IsCancellationRequested &&
                       !NotifyWhenAllNodesAreUp.IsSet)
                {
                    using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                    {
                        sqlConnection.Open();

                        using (SqlCommand command =
                            new SqlCommand("SELECT COUNT(*) FROM Stardust.WorkerNodes",
                                           sqlConnection))
                        {
                            int rowCount = (int) command.ExecuteScalar();

                            if (nodes == rowCount)
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

            return NotifyWhenAllNodesAreUpTask;
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
            while (NotifyWhenAllNodesAreUpTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }

            NotifyWhenAllNodesAreUpTask.Dispose();

            LogHelper.LogDebugWithLineNumber("Finished dispose.",
                                             Logger);
        }
    }
}