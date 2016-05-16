using System.Data.SqlClient;
using System.Timers;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class PurgeTimer : Timer
	{
		private readonly ManagerConfiguration _managerConfiguration;
		private readonly RetryPolicy _retryPolicy;
		private readonly string _connectionString;


		public PurgeTimer(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(managerConfiguration.PurgeIntervalHours*60*60*1000)
		{
			_managerConfiguration = managerConfiguration;
			_retryPolicy = retryPolicyProvider.GetPolicy();
			_connectionString = managerConfiguration.ConnectionString;
			Elapsed += PurgeTimer_elapsed;
			Purge();
		}

		private void PurgeTimer_elapsed(object sender, ElapsedEventArgs e)
		{
			Purge();
		}

		private void Purge()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				string deleteCommandText = "DELETE TOP(@batchsize) FROM [Stardust].[Job] WHERE Created < DATEADD(HOUR, -@hours, GETDATE())";
				connection.OpenWithRetry(_retryPolicy);
				using (var deleteCommand = new SqlCommand(deleteCommandText, connection))
				{
					deleteCommand.Parameters.AddWithValue("@hours", _managerConfiguration.PurgeJobsOlderThanHours);
					deleteCommand.Parameters.AddWithValue("@batchsize", _managerConfiguration.PurgeBatchSize);

					deleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
		}
	}
}
