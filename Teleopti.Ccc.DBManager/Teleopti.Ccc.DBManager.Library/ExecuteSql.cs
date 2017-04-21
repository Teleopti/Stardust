using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class ExecuteSql
	{
		private readonly Func<SqlConnection> _openConnection;
		private readonly IUpgradeLog _upgradeLog;
		private readonly RetryPolicy<SqlTransientErrorDetectionStrategyWithTimeouts> _retryPolicy;

		public ExecuteSql(Func<SqlConnection> openConnection, IUpgradeLog upgradeLog)
		{
			_openConnection = openConnection;
			_upgradeLog = upgradeLog;
			var retryStrategy = new Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
			_retryPolicy = new RetryPolicy<SqlTransientErrorDetectionStrategyWithTimeouts>(retryStrategy);
			_retryPolicy.Retrying += (sender, args) =>
			{
				var msg = string.Format("Retrying - Count: {0}, Delay: {1}, Exception: {2}", args.CurrentRetryCount, args.Delay, args.LastException);
				_upgradeLog.Write(msg, "WARN");
			};
		}

		public void Execute(Action<SqlConnection> action)
		{
			_retryPolicy.ExecuteAction(() =>
			{
				using (var connection = _openConnection())
				{
					action(connection);
				}
			});

		}

		public void Execute(string sql)
		{
			_retryPolicy.ExecuteAction(() =>
			{
				using (var connection = _openConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.ExecuteNonQuery();
				}
			});
		}

		public int ExecuteScalar(string sql, int timeout = 30, IDictionary<string, object> parameters = null)
		{
			parameters = parameters ?? new Dictionary<string, object>();
			var result = 0;

			handleWithRetry(() => sql, () =>
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
			});
			return result;
		}

		public void ExecuteTransactionlessNonQuery(string sql, int timeout = 30)
		{
			string logSql = null;

			handleWithRetry(() => logSql, () =>
			{
				var regex = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				var allScripts = regex.Split(sql);
				var statements = allScripts.Where(x => !string.IsNullOrWhiteSpace(x));

				using (var connection = _openConnection())
				{
					foreach (var statement in statements)
					{
						logSql = statement;
						using (var command = connection.CreateCommand())
						{
							command.CommandType = CommandType.Text;
							command.CommandTimeout = timeout;
							command.CommandText = statement;
							command.ExecuteNonQuery();
						}
					}

				}
			});
		}
		
		public void ExecuteNonQuery(string sql, int timeout = 30, IDictionary<string, object> parameters = null)
		{
			parameters = parameters ?? new Dictionary<string, object>();
			string logSql = null;

			handleWithRetry(() => logSql, () =>
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
							command.CommandTimeout = timeout;

							foreach (var statement in statements)
							{
								logSql = statement;
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
			});
		}

		private void handleWithRetry(Func<string> logSql, Action action)
		{
			try
			{
				_retryPolicy.ExecuteAction(action);
			}
			catch (Exception exception)
			{
				var message = exception.Message;
				SqlException sqlException;
				if ((sqlException = exception as SqlException) != null)
				{
					message = $"{message}{Environment.NewLine}Error numbers: {string.Join(", ", sqlException.Errors.OfType<SqlError>().Select(e => e.Number))}";
				}

				_upgradeLog.Write(message, "ERROR");
				_upgradeLog.Write("Failing script: " + logSql.Invoke(), "ERROR");

				throw;
			}
		}
	}
}