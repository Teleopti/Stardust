using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ManagerTest.Database
{
	public class ExecuteSql
	{
		private readonly SqlTransientErrorChecker _errorChecker = new SqlTransientErrorChecker();
		private readonly Func<SqlConnection> _openConnection;

		public ExecuteSql(Func<SqlConnection> openConnection)
		{
			_openConnection = openConnection;
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

			var result = 0;
			HandleWithRetry(sql, s =>
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
								result = (int) (command.ExecuteScalar() ?? default(int));
							}

							transaction.Commit();
						}
					}
				}
			}, 0);

			return result;
		}

		public void ExecuteTransactionlessNonQuery(string sql, int timeout = 30)
		{
			HandleWithRetry(sql, s =>
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

			HandleWithRetry(sql, s =>
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

		private void HandleWithRetry(string sql, Action<string> action, int attempt)
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
					HandleWithRetry(sql, action, ++attempt);
				}

				var message = exception.Message;
				SqlException sqlException;
				if ((sqlException = exception as SqlException) != null)
				{
					message = message + Environment.NewLine + "Error numbers: " +
					          string.Join(", ", sqlException.Errors.OfType<SqlError>().Select(e => e.Number));
				}

				throw;
			}
		}
	}

	public class SqlTransientErrorChecker
	{
		public bool IsTransient(Exception ex)
		{
			if (ex != null)
			{
				SqlException sqlException;
				if ((sqlException = ex as SqlException) != null)
				{
					// Enumerate through all errors found in the exception.
					foreach (SqlError err in sqlException.Errors)
					{
						switch (err.Number)
						{
							case -2: //Timeout!
							case 64:
							case 233:
							case 4060:
							case 10053:
							case 10054:
							case 10060:
							case 10928:
							case 10929:
							case 40501:
							case 40143:
							case 40197:
							case 40540:
							case 40613:
							case 40648:
							case 40671:
							case 42019:
							case 45168:
							case 45169:
							case 49918:
							case 49919:
							case 49920:
								return true;
						}
					}
				}
				else if (ex is TimeoutException)
				{
					return true;
				}
			}

			return false;
		}
	}
}