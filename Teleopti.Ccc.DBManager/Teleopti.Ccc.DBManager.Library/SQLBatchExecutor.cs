using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.DBManager.Library
{
	public interface ILog
	{
		void Write(string message);
	}

	public class SQLBatchExecutor
	{
		private readonly SqlConnection _sqlConnection;
		private readonly ILog _log;

		public SQLBatchExecutor(SqlConnection sqlConnection, ILog log)
		{
			_sqlConnection = sqlConnection;
			_log = log;
		}

		public void ExecuteBatchSQL(string sql)
		{
			SqlTransaction transaction = _sqlConnection.BeginTransaction();
			Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			string[] lines = regex.Split(sql);

			using (SqlCommand sqlCommand = _sqlConnection.CreateCommand())
			{
				sqlCommand.Connection = _sqlConnection;
				sqlCommand.CommandTimeout = Timeouts.CommandTimeout;
				sqlCommand.Transaction = transaction;

				foreach (string line in lines)
				{
					if (line.Length > 0)
					{
						sqlCommand.CommandText = line;
						sqlCommand.CommandType = CommandType.Text;

						try
						{
							sqlCommand.ExecuteNonQuery();
						}
						catch (SqlException e)
						{
							_log.Write(e.Message);
							transaction.Rollback();
							break;
							//In this case stop the whole thing
						}
					}
				}
				transaction.Commit();
			}
		}
	}
}
