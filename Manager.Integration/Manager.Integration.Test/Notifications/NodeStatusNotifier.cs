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

        public string ConnectionString { get; set; }

        public ManualResetEventSlim JobDefinitionStatusNotify { get; set; }

        public NodeStatusNotifier(string connectionString)
        {
            ConnectionString = connectionString;

            JobDefinitionStatusNotify = new ManualResetEventSlim();
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
                               !JobDefinitionStatusNotify.IsSet)
                        {
                            int numberOfRows = (int) command.ExecuteScalar();

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
    }
}