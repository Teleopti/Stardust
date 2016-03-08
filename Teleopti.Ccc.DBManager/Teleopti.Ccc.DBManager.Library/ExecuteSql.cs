using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class ExecuteSql
	{
		private readonly Func<SqlConnection> _openConnection;
		private readonly IUpgradeLog _upgradeLog;
		private readonly SqlTransientErrorChecker _errorChecker = new SqlTransientErrorChecker();

		public ExecuteSql(Func<SqlConnection> openConnection, IUpgradeLog upgradeLog)
		{
			_openConnection = openConnection;
			_upgradeLog = upgradeLog;
		}

		public void Execute(Action<SqlConnection> action)
		{
			using (var connection = _openConnection())
			{
				action(connection);
			}
		}

		public void Execute(string sql)
		{
			using (var connection = _openConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}

		public int ExecuteScalar(string sql, int timeout = 30, IDictionary<string, object> parameters = null)
		{
			parameters = parameters ?? new Dictionary<string, object>();

			int result = 0;
			handleWithRetry(sql, s =>
			{
				var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				var allScripts = regex.Split(sql);
				var statements = allScripts.Where(x => !string.IsNullOrWhiteSpace(x));

				using (var connection = _openConnection())
				{
					using (var transaction = connection.BeginTransaction())
					{
						using (var command = connection.CreateCommand())
						{
							command.Transaction = transaction;
							
							foreach (var parameter in parameters)
							{
								command.Parameters.AddWithValue(parameter.Key, parameter.Value);
							}
							command.CommandTimeout = timeout;

							foreach (var statement in statements)
							{
								command.CommandText = statement;
								command.CommandType = CommandType.Text;
								result = (int)(command.ExecuteScalar() ?? default(int));
							}
							
							transaction.Commit();
						}

					}
				}
			},0);

			return result;
		}

		public void ExecuteTransactionlessNonQuery(string sql, int timeout = 30)
		{
			handleWithRetry(sql, s =>
			{
				using (var connection = _openConnection())
				{
					using (var command = connection.CreateCommand())
					{
						command.CommandType = CommandType.Text;
						command.CommandTimeout = timeout;
						command.CommandText = sql;
						command.ExecuteNonQuery();
					}
				}
			}, 0);
		}

		public void ExecuteNonQuery(string sql, int timeout = 30, IDictionary<string, object> parameters = null)
		{
			parameters = parameters ?? new Dictionary<string, object>();

			var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var allScripts = regex.Split(sql);
			var statements = allScripts.Where(x => !string.IsNullOrWhiteSpace(x));

			handleWithRetry(sql, s =>
			{
				using (var connection = _openConnection())
				{
					using (var transaction = connection.BeginTransaction())
					{
						using (var command = connection.CreateCommand())
						{
							command.Transaction = transaction;
							command.CommandTimeout = timeout;

							foreach (var statement in statements)
							{
								command.Parameters.Clear();
								foreach (var parameter in parameters)
								{
									if (statement.Contains(parameter.Key))
									{
										command.Parameters.AddWithValue(parameter.Key, parameter.Value);
									}
								}
								command.CommandText = statement;
								command.CommandType = CommandType.Text;
								command.ExecuteNonQuery();
							}
							
							transaction.Commit();
						}
					}
				}
			}, 0);
		}

		private void handleWithRetry(string sql, Action<string> action, int attempt)
		{
			try
			{
				action.Invoke(sql);
			}
			catch (Exception exception)
			{
				if (attempt < 6 && _errorChecker.IsTransient(exception))
				{
					Thread.Sleep(5);
					handleWithRetry(sql, action, ++attempt);
				}

				string message = exception.Message;
				SqlException sqlException;
				if ((sqlException = exception as SqlException) != null)
				{
					message = message + Environment.NewLine + "Error numbers: " +
							  string.Join(", ", sqlException.Errors.OfType<SqlError>().Select(e => e.Number));
				}

				_upgradeLog.Write(message, "ERROR");
				_upgradeLog.Write("Failing script: " + sql, "ERROR");
				
				throw;
			}
		}
	}
}