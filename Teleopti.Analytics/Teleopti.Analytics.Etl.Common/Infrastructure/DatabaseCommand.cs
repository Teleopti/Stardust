using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class DatabaseCommand
	{
		private readonly string _commandText;
		private readonly CommandType _commandType;
		private readonly string _connString;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();

		public DatabaseCommand(CommandType commandType, string commandText, string connectionString)
		{
			_connString = connectionString;
			_commandType = commandType;
			_commandText = commandText;
		}
		
		public DataSet ExecuteDataSet(SqlParameter[] sqlParameters)
		{
			using (var ds = new DataSet())
			{
				using (var dataAdapter = new SqlDataAdapter())
				{
					_executor.Run(grabConnection, conn =>
					{
						using (var tran = OpenTransaction(conn))
						{
							using (var command = SetCommand(tran))
							{
								ds.Locale = Thread.CurrentThread.CurrentCulture;
								setParams(command, sqlParameters);
								dataAdapter.SelectCommand = command;
								dataAdapter.Fill(ds);
								tran.Commit();
							}
						}
					});
				}
				return ds;
			}
		}

		public int ExecuteNonQuery(SqlParameter[] sqlParameters)
		{
			int number = default(int);
			_executor.Run(grabConnection, conn =>
			{
				using (var tran = OpenTransaction(conn))
				{
					using (var command = SetCommand(tran))
					{
						setParams(command, sqlParameters);
						number = command.ExecuteNonQuery();
						tran.Commit();
					}
				}
			}); 
			return number;
		}

		public object ExecuteScalar(params SqlParameter[] sqlParameters)
		{
			object retVal = default(object);
			_executor.Run(grabConnection, conn =>
			{
				using (var tran = OpenTransaction(conn))
				{
					using (var command = SetCommand(tran))
					{
						setParams(command, sqlParameters);
						retVal = command.ExecuteScalar();
						tran.Commit();
					}
				}
			});
			return retVal;
		}

		private void setParams(SqlCommand command, SqlParameter[] sqlParameters)
		{
			command.Parameters.Clear();
			foreach (var sqlParameter in sqlParameters)
			{
				command.Parameters.AddWithValue(sqlParameter.ParameterName, sqlParameter.Value ?? DBNull.Value);
			}
		}

		public int ExecuteNonQueryMaintenance()
		{
			int number = default (int);
			_executor.Run(grabConnection, conn =>
			{
				using (var command = SetCommandMaintenance(conn))
				{
					number = command.ExecuteNonQuery();
				}
			});
			return number;
		}

		protected virtual SqlCommand SetCommandMaintenance(SqlConnection conn)
		{
			const string timeout = "21600";
			return new SqlCommand
				{
					CommandText = _commandText,
					Connection = conn,
					CommandType = _commandType,
					CommandTimeout = int.Parse(timeout, CultureInfo.InvariantCulture)
				};
		}

		protected virtual SqlCommand SetCommand(SqlTransaction transaction)
		{
			var timeout = "60";
			if (ConfigurationManager.AppSettings["databaseTimeout"] != null)
				timeout = ConfigurationManager.AppSettings["databaseTimeout"];
			return new SqlCommand
				{
					CommandText = _commandText,
					Transaction = transaction,
					Connection = transaction.Connection,
					CommandType = _commandType,
					CommandTimeout = int.Parse(timeout, CultureInfo.InvariantCulture)
				};
		}

		private SqlConnection grabConnection()
		{
			var conn = new SqlConnection(_connString);
			conn.Open();
			return conn;
		}

		private static SqlTransaction OpenTransaction(SqlConnection connection)
		{
			return connection.BeginTransaction();
		}
	}
}
