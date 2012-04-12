using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

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
			sqlText += string.Format("select {0},{1},{2},{3}", computerName, startTime, jobName, isStartByService);
			sqlCommand.CommandType = CommandType.Text;
			sqlCommand.CommandText = sqlText;
			sqlCommand.ExecuteNonQuery();

		}

		public void Dispose()
		{
			_sqlConnection.Dispose();

		}
	}
}
