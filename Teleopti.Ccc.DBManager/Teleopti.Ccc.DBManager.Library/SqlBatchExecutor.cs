using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.DBManager.Library
{
	public class SqlBatchExecutor
	{
		private readonly SqlConnection _sqlConnection;
		private readonly ILog _log;

		public SqlBatchExecutor(SqlConnection sqlConnection, ILog log)
		{
			_sqlConnection = sqlConnection;
			_log = log;
		}

		public void ExecuteBatchSql(string sql)
		{
			var transaction = _sqlConnection.BeginTransaction();
			var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var allScripts = regex.Split(sql);
			var applicableScripts = from s in allScripts where s.Length > 0 select s;

			using (var sqlCommand = _sqlConnection.CreateCommand())
			{
				sqlCommand.Connection = _sqlConnection;
				sqlCommand.CommandTimeout = Timeouts.CommandTimeout;
				sqlCommand.Transaction = transaction;

				foreach (var script in applicableScripts)
				{
					sqlCommand.CommandText = script;
					sqlCommand.CommandType = CommandType.Text;

					try
					{
						sqlCommand.ExecuteNonQuery();
					}
					catch (SqlException e)
					{
						_log.Write(e.Message);
						_log.Write("Failing script:");
						_log.Write(script);
						transaction.Rollback();
						throw new ApplicationException("Sql script failed: " + e);
					}
				}
				transaction.Commit();
			}
		}
	}
}
