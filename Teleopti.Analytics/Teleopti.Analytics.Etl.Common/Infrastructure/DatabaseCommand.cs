using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class DatabaseCommand
	{
		private readonly string _commandText;
		private readonly CommandType _commandType;
		private readonly string _connString;
		private readonly IList<SqlParameter> _procParam;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();

		public DatabaseCommand(CommandType commandType, string commandText, string connectionString)
		{
			_connString = connectionString;
			_commandType = commandType;
			_commandText = commandText;
			_procParam = new List<SqlParameter>();
		}
		
		public void AddProcParameter(SqlParameter parameter)
		{
			InParameter.NotNull("parameter", parameter);

			if (parameter.Value == null)
				parameter.Value = DBNull.Value;

			_procParam.Add(parameter);
		}
		
		public DataSet ExecuteDataSet()
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
								setParams(command);
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

		public int ExecuteNonQuery()
		{
			int number = default(int);
			_executor.Run(grabConnection, conn =>
			{
				using (var tran = OpenTransaction(conn))
				{
					using (var command = SetCommand(tran))
					{
						setParams(command);
						number = command.ExecuteNonQuery();
						tran.Commit();
					}
				}
			}); 
			return number;
		}

		public object ExecuteScalar()
		{
			object retVal = default(object);
			_executor.Run(grabConnection, conn =>
			{
				using (var tran = OpenTransaction(conn))
				{
					using (var command = SetCommand(tran))
					{
						setParams(command);
						retVal = command.ExecuteScalar();
						tran.Commit();
					}
				}
			});
			return retVal;
		}

		public int ExecuteNonQueryMaintenance()
		{
			int number = default (int);
			_executor.Run(grabConnection, conn =>
			{
				using (var command = SetCommandMaintenance(conn))
				{
					setParams(command);
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

		private void setParams(SqlCommand command)
		{
			int num2 = _procParam.Count - 1;
			for (int i = 0; i <= num2; i++)
			{
				command.Parameters.Add(_procParam[i]);
			}
		}
	}
}
