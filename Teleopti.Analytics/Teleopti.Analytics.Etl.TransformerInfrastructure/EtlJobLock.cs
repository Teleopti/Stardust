using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public class EtlJobLock: IEtlJobLock
	{
		private readonly string _connectionString;
		private SqlConnection _sqlConnection;

		public EtlJobLock(string connectionString)
		{
			_connectionString = connectionString;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreateLock(string jobName, bool isStartByService)
		{
			_sqlConnection= new SqlConnection(_connectionString);
			_sqlConnection.Open();
			SqlTransaction sqlTransaction = _sqlConnection.BeginTransaction();

			SqlCommand sqlCommand = _sqlConnection.CreateCommand();
			sqlCommand.Transaction = sqlTransaction;

			string computerName = Environment.MachineName;
			DateTime startTime = DateTime.Now;
			string sqlText = "insert into mart.sys_etl_running_lock ";
			sqlText += string.Format(CultureInfo.InvariantCulture, "select '{0}','{1}','{2}',{3}", computerName, startTime, jobName, isStartByService ? 1 : 0);
			sqlCommand.CommandType = CommandType.Text;
			sqlCommand.CommandText = sqlText;
			sqlCommand.ExecuteNonQuery();

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

		protected virtual void ReleaseManagedResources()
		{
			_sqlConnection.Dispose();
		}
	}
}
