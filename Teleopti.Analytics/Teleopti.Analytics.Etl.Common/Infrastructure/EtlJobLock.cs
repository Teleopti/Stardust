using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class EtlJobLock: IEtlJobLock
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (EtlJobLock));
		private Timer timer;
		private readonly Func<SqlConnection> _connection;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();
		const string insertStatement = "INSERT INTO mart.sys_etl_running_lock (computer_name,start_time,job_name,is_started_by_service,lock_until) VALUES (@computer_name,@start_time,@job_name,@is_started_by_service,DATEADD(mi,1,GETUTCDATE()))";
		const string updateStatement = "UPDATE mart.sys_etl_running_lock SET lock_until=DATEADD(mi,1,GETUTCDATE())";
		const string deleteStatement = "DELETE FROM mart.sys_etl_running_lock";
		
		public EtlJobLock(string connectionString)
		{
			_connection = () =>
			{
				var conn = new SqlConnection(connectionString);
				conn.Open();
				return conn;
			};
		}

		public void CreateLock(string jobName, bool isStartByService)
		{
			_executor.Run(_connection, conn =>
			{
				SqlTransaction sqlTransaction = conn.BeginTransaction();

				SqlCommand sqlCommand = conn.CreateCommand();
				sqlCommand.Transaction = sqlTransaction;

				string computerName = Environment.MachineName;

				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = insertStatement;
				sqlCommand.Parameters.AddWithValue("@computer_name", computerName);
				sqlCommand.Parameters.AddWithValue("@start_time", DateTime.Now);
				sqlCommand.Parameters.AddWithValue("@job_name", jobName);
				sqlCommand.Parameters.AddWithValue("@is_started_by_service", isStartByService);
				sqlCommand.ExecuteNonQuery();

				sqlTransaction.Commit();
			});

			timer = new Timer(lockForAnotherMinute,null,TimeSpan.FromSeconds(45),TimeSpan.FromMinutes(1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void lockForAnotherMinute(object state)
		{
			try
			{
				_executor.Run(_connection, conn =>
				{
					SqlTransaction sqlTransaction = conn.BeginTransaction();

					SqlCommand sqlCommand = conn.CreateCommand();
					sqlCommand.Transaction = sqlTransaction;

					sqlCommand.CommandType = CommandType.Text;
					sqlCommand.CommandText = updateStatement;
					sqlCommand.ExecuteNonQuery();

					sqlTransaction.Commit();
				});
			}
			catch (Exception exception)
			{
				Logger.Error("Got an error when trying to extend the lock. Will try again in 10 seconds.",exception);
				timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
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
			try
			{
				_executor.Run(_connection, conn =>
				{
					SqlTransaction sqlTransaction = conn.BeginTransaction();

					SqlCommand sqlCommand = conn.CreateCommand();
					sqlCommand.Transaction = sqlTransaction;

					sqlCommand.CommandType = CommandType.Text;
					sqlCommand.CommandText = deleteStatement;
					sqlCommand.ExecuteNonQuery();

					sqlTransaction.Commit();
				});
			}
			catch (Exception)
			{
				//Suppress any errors in here...
			}
			timer.Dispose();
		}
	}
}
