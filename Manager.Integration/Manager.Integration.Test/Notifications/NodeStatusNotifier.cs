using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Manager.Integration.Test.Notifications
{
    public class NodeStatusNotifier : IDisposable
    {
        public string ConnectionString { get; set; }

        public ManualResetEventSlim NotifyWhenNodeStarted { get; set; }

        public NodeStatusNotifier(string connectionString)
        {
            ConnectionString = connectionString;

            NotifyWhenNodeStarted = new ManualResetEventSlim();
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private Task JobDefinitionStatusNotifierTask { get; set; }

        public void StartJobDefinitionStatusNotifier(Guid guid,
                                                     string status,
                                                     CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;

            JobDefinitionStatusNotifierTask = Task.Factory.StartNew(() =>
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string sqlCommand =
                        @"SELECT COUNT(*) FROM Stardust.JobDefinitions 
                            WHERE Id = @Id AND Status = @Status";

                    using (SqlCommand command = new SqlCommand(sqlCommand,
                                                               connection))
                    {
                        command.Parameters.AddWithValue("@Id",
                                                        guid);

                        command.Parameters.AddWithValue("@Status",
                                                        status);

                        while (!cancellationTokenSource.IsCancellationRequested &&
                               !NotifyWhenNodeStarted.IsSet)
                        {
                            int numberOfRows = (int) command.ExecuteScalar();

                            if (numberOfRows == 1)
                            {
                                NotifyWhenNodeStarted.Set();
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

        public void Dispose()
        {
            if (CancellationTokenSource != null &&
                !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            if (JobDefinitionStatusNotifierTask != null)
            {
                JobDefinitionStatusNotifierTask.Dispose();
            }
        }
    }
}