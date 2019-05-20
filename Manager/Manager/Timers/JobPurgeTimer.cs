using System.Data.SqlClient;
using System.Timers;
using Polly.Retry;

namespace Stardust.Manager.Timers
{
	public class JobPurgeTimer : Timer
	{
		private readonly ManagerConfiguration _managerConfiguration;
		private readonly RetryPolicy _retryPolicy;
		private readonly string _connectionString;


		public JobPurgeTimer(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(managerConfiguration.PurgeJobsIntervalHours*60*60*1000)
		{
			_managerConfiguration = managerConfiguration;
			_retryPolicy = retryPolicyProvider.GetPolicy();
			_connectionString = managerConfiguration.ConnectionString;
			Elapsed += PurgeTimer_elapsed;
		}

		private void PurgeTimer_elapsed(object sender, ElapsedEventArgs e)
		{
			Purge();
		}

		public virtual void Purge()
		{
			const string deleteCommandText = "DELETE TOP(@batchsize) FROM [Stardust].[Job] WHERE Created < DATEADD(HOUR, -@hours, GETDATE())";
			using (var connection = new SqlConnection(_connectionString))
			{
                _retryPolicy.Execute(connection.Open);
				using (var deleteCommand = new SqlCommand(deleteCommandText, connection))
				{
					deleteCommand.Parameters.AddWithValue("@hours", _managerConfiguration.PurgeJobsOlderThanHours);
					deleteCommand.Parameters.AddWithValue("@batchsize", _managerConfiguration.PurgeJobsBatchSize);

                    _retryPolicy.Execute(deleteCommand.ExecuteNonQuery);
				}
			}
		}
	}
}
