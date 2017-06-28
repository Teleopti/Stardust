using System.Data.SqlClient;
using System.Timers;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager.Timers
{
	public class NodePurgeTimer : Timer
	{
		private readonly RetryPolicy _retryPolicy;
		private readonly string _connectionString;

		public NodePurgeTimer(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(managerConfiguration.PurgeNodesIntervalHours*60*60*1000)
		{
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
			const string deleteCommandText = "DELETE FROM [Stardust].[WorkerNode] WHERE Alive = 0";
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var deleteCommand = new SqlCommand(deleteCommandText, connection))	
				{
					deleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
		}
	}
}