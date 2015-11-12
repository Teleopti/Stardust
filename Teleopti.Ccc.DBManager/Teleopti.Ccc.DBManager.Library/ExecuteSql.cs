using System;
using System.Collections.Generic;
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

		public void ExecuteCustom(Action<SqlConnection> action)
		{
			using (var connection = _openConnection())
			{
				action(connection);
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
								result = (int)command.ExecuteScalar();
							}
							
							transaction.Commit();
						}

					}
				}
			},0);

			return result;
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
							foreach (var parameter in parameters)
							{
								command.Parameters.AddWithValue(parameter.Key, parameter.Value);
							}
							command.CommandTimeout = timeout;

							foreach (var statement in statements)
							{
								command.CommandText = statement;
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

				_upgradeLog.Write(exception.Message, "ERROR");
				_upgradeLog.Write("Failing script: " + sql, "ERROR");
				
				throw;
			}
		}
	}
}