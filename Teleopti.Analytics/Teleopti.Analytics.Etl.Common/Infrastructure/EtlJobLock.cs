using System;
using System.Data.SqlClient;
using System.Timers;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class EtlJobLock : IEtlJobLock
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(EtlJobLock));
		private Timer timer;
		private readonly Func<SqlConnection> _connection;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();
		private readonly IDisposable sqlServerDistributedLock;
		private const int lockTimeMinutes = 5;

		public EtlJobLock(string connectionString, string jobName, bool isStartByService)
		{
			_connection = () =>
			{
				var conn = new SqlConnection(connectionString);
				conn.Open();
				return conn;
			};

			// Aquire a distributed lock
			sqlServerDistributedLock = new SqlMonitor().Enter("ETLJobLock", TimeSpan.Zero, _connection());
			createLock(jobName, isStartByService);
		}

		private void createLock(string jobName, bool isStartByService)
		{
			var insertStatement = $"INSERT INTO mart.sys_etl_running_lock (computer_name,start_time,job_name,is_started_by_service,lock_until) VALUES (@computer_name,@start_time,@job_name,@is_started_by_service,DATEADD(mi,{lockTimeMinutes},GETUTCDATE()))";
			_executor.Run(_connection, conn =>
			{
				var sqlTransaction = conn.BeginTransaction();
				using (var command = new SqlCommand(insertStatement, conn, sqlTransaction))
				{
					command.Parameters.AddWithValue("@computer_name", Environment.MachineName);
					command.Parameters.AddWithValue("@start_time", DateTime.Now);
					command.Parameters.AddWithValue("@job_name", jobName);
					command.Parameters.AddWithValue("@is_started_by_service", isStartByService);
					command.ExecuteNonQuery();
				}
				sqlTransaction.Commit();
			});

			timer = new Timer(60000);
			timer.Elapsed += pushLockForward;
			timer.Start();
		}

		private void pushLockForward(object source, ElapsedEventArgs e)
		{
			var updateStatement = $"UPDATE mart.sys_etl_running_lock SET lock_until=DATEADD(mi,{lockTimeMinutes},GETUTCDATE())";
			try
			{
				_executor.Run(_connection, conn =>
				{
					var sqlTransaction = conn.BeginTransaction();
					using (var command = new SqlCommand(updateStatement, conn, sqlTransaction))
					{
						command.ExecuteNonQuery();
					}
					sqlTransaction.Commit();
				});
			}
			catch (Exception exception)
			{
				logger.Error("Got an error when trying to extend the lock. Will try again the next minute. ", exception);
			}
		}

		public void Dispose()
		{
			dispose(true);
			sqlServerDistributedLock.Dispose();
			GC.SuppressFinalize(this);
		}

		private void dispose(bool disposing)
		{
			timer.Dispose();
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected virtual void ReleaseManagedResources()
		{
			const string deleteStatement = "DELETE FROM mart.sys_etl_running_lock";
			try
			{
				_executor.Run(_connection, conn =>
				{
					var sqlTransaction = conn.BeginTransaction();
					using (var command = new SqlCommand(deleteStatement, conn, sqlTransaction))
					{
						command.ExecuteNonQuery();
					}
					sqlTransaction.Commit();
				});
			}
			catch (Exception)
			{
				//Suppress any errors in here...
			}
		}
	}
}
