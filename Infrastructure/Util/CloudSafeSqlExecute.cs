using System;
using System.Data.SqlClient;
using System.Threading;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class CloudSafeSqlExecute
	{
		private readonly SqlTransientErrorChecker _errorChecker = new SqlTransientErrorChecker();

		public void Run(Func<SqlConnection> connection, Action<SqlConnection> action)
		{
			handleWithRetry(connection,action,0);
		}

		private void handleWithRetry(Func<SqlConnection> connection, Action<SqlConnection> action, int attempt)
		{
			try
			{
				using (var conn = connection())
				{
					action.Invoke(conn);
				}
			}
			catch (Exception exception)
			{
				if (attempt < 6 && _errorChecker.IsTransient(exception))
				{
					Thread.Sleep(5);
					handleWithRetry(connection, action, ++attempt);
				}

				throw;
			}
		}
	}
}