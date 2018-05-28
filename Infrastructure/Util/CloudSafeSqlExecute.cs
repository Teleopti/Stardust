using System;
using System.Data.SqlClient;
using Polly;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class CloudSafeSqlExecute
	{
		public void Run(Func<SqlConnection> connection, Action<SqlConnection> action)
		{
			Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(5, i => TimeSpan.FromMilliseconds(5))
				.Execute(() =>
				{
					using (var conn = connection())
					{
						action.Invoke(conn);
					}
				});
		}
	}
}