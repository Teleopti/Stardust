using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Manager.Integration.Test.Notifications
{
    public class SqlNotifier : IDisposable
    {
        public string ConnectionString { get; set; }

        public SqlNotifier(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ManualResetEventSlim NotifyWhenAllNodesAreUp { get; set; }

        private CancellationTokenSource NotifyWhenAllNodesAreUpTaskCancellationTokenSource { get; set; }

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

            return new Task(() =>
            {
                int nodes = numberOfNodes;

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                    {
                        sqlConnection.Open();

                        using (SqlCommand command =  
                                new SqlCommand("SELECT COUNT(*) FROM Stardust.WorkerNodes", sqlConnection))
                        {
                            int rowCount = (int)command.ExecuteScalar();

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

            }, NotifyWhenAllNodesAreUpTaskCancellationTokenSource.Token);
        }

        public void Dispose()
        {
            if (NotifyWhenAllNodesAreUpTaskCancellationTokenSource != null &&
                !NotifyWhenAllNodesAreUpTaskCancellationTokenSource.IsCancellationRequested)
            {
                NotifyWhenAllNodesAreUpTaskCancellationTokenSource.Cancel();
            }
        }
    }
}