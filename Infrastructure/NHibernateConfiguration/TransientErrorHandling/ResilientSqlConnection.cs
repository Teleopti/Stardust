using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using Polly;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public sealed class ResilientSqlConnection : IDbConnection, IDisposable, ICloneable
	{
		private readonly Policy connectionStringFailoverPolicy;
		private string connectionString;
		
		public ResilientSqlConnection(string connectionString, Policy connectionRetryPolicy, Policy commandRetryPolicy)
		{
			this.connectionString = connectionString;
			Current = new SqlConnection(connectionString);
			ConnectionRetryPolicy = connectionRetryPolicy;
			CommandRetryPolicy = commandRetryPolicy;
			connectionStringFailoverPolicy = Policy.Handle<SqlException>(NetworkConnectivityErrorDetectionStrategy)
				.WaitAndRetry(1, i => TimeSpan.FromMilliseconds(1.0));
		}

		public Policy ConnectionRetryPolicy { get; }

		public Policy CommandRetryPolicy { get; }

		public SqlConnection Current { get; }

		public Guid SessionTracingId
		{
			get
			{
				try
				{
					using (var contextInfoCommand = SqlCommandFactory.CreateGetContextInfoCommand(Current))
					{
						return ExecuteCommand<Guid>(contextInfoCommand);
					}
				}
				catch
				{
					return Guid.Empty;
				}
			}
		}

		object ICloneable.Clone()
		{
			return new ResilientSqlConnection(ConnectionString, ConnectionRetryPolicy, CommandRetryPolicy);
		}

		public string ConnectionString
		{
			get => connectionString;
			set
			{
				connectionString = value;
				Current.ConnectionString = value;
			}
		}

		public int ConnectionTimeout => Current.ConnectionTimeout;

		public string Database => Current.Database;

		public ConnectionState State => Current.State;

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return Current.BeginTransaction(il);
		}

		public IDbTransaction BeginTransaction()
		{
			return Current.BeginTransaction();
		}

		public void ChangeDatabase(string databaseName)
		{
			Current.ChangeDatabase(databaseName);
		}

		void IDbConnection.Open()
		{
			Open();
		}

		public void Close()
		{
			Current.Close();
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return Current.CreateCommand();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public SqlConnection Open()
		{
			return Open(ConnectionRetryPolicy);
		}

		public SqlConnection Open(Policy retryPolicy)
		{
			(retryPolicy ?? Policy.NoOp()).Execute(() => connectionStringFailoverPolicy.Execute(() =>
			{
				if (Current.State == ConnectionState.Open)
					return;
				Current.Open();
			}));
			return Current;
		}

		public T ExecuteCommand<T>(IDbCommand command)
		{
			return ExecuteCommand<T>(command, CommandRetryPolicy, CommandBehavior.Default);
		}

		public T ExecuteCommand<T>(IDbCommand command, CommandBehavior behavior)
		{
			return ExecuteCommand<T>(command, CommandRetryPolicy, behavior);
		}

		public T ExecuteCommand<T>(IDbCommand command, Policy retryPolicy)
		{
			return ExecuteCommand<T>(command, retryPolicy, CommandBehavior.Default);
		}

		public T ExecuteCommand<T>(IDbCommand command, Policy retryPolicy, CommandBehavior behavior)
		{
			var actionResult = default(T);
			var resultType = typeof(T);
			var hasOpenedConnection = false;
			var closeOpenedConnectionOnSuccess = false;
			try
			{
				(retryPolicy ?? Policy.NoOp()).Execute((Action) (() => actionResult = connectionStringFailoverPolicy.Execute(() =>
				{
					if (command.Connection == null)
					{
						command.Connection = Open();
						hasOpenedConnection = true;
					}

					if (command.Connection.State != ConnectionState.Open)
					{
						command.Connection.Open();
						hasOpenedConnection = true;
					}

					if (typeof(IDataReader).IsAssignableFrom(resultType))
					{
						closeOpenedConnectionOnSuccess = false;
						return (T) command.ExecuteReader(behavior);
					}

					if (resultType == typeof(XmlReader))
					{
						if (!(command is SqlCommand))
							throw new NotSupportedException();
						var innerReader = (command as SqlCommand).ExecuteXmlReader();
						closeOpenedConnectionOnSuccess = false;
						return (T) ((behavior & CommandBehavior.CloseConnection) != CommandBehavior.CloseConnection
							? (object) innerReader
							: (object) new SqlXmlReader(command.Connection, innerReader));
					}

					if (resultType == typeof(NonQueryResult))
					{
						var nonQueryResult = new NonQueryResult
						{
							RecordsAffected = command.ExecuteNonQuery()
						};
						closeOpenedConnectionOnSuccess = true;
						return (T) Convert.ChangeType(nonQueryResult, resultType, CultureInfo.InvariantCulture);
					}

					var obj = command.ExecuteScalar();
					closeOpenedConnectionOnSuccess = true;
					if (obj != null)
						return (T) Convert.ChangeType(obj, resultType, CultureInfo.InvariantCulture);
					return default(T);
				})));
				if (hasOpenedConnection)
					if (closeOpenedConnectionOnSuccess)
						if (command.Connection != null)
							if (command.Connection.State == ConnectionState.Open)
								command.Connection.Close();
			}
			catch (Exception)
			{
				if (hasOpenedConnection && command.Connection != null && command.Connection.State == ConnectionState.Open)
					command.Connection.Close();
				throw;
			}

			return actionResult;
		}

		public int ExecuteCommand(IDbCommand command)
		{
			return ExecuteCommand(command, CommandRetryPolicy);
		}

		public int ExecuteCommand(IDbCommand command, Policy retryPolicy)
		{
			return ExecuteCommand<NonQueryResult>(command, retryPolicy).RecordsAffected;
		}

		public SqlCommand CreateCommand()
		{
			return Current.CreateCommand();
		}

		private void Dispose(bool disposing)
		{
			if (!disposing)
				return;
			if (Current.State == ConnectionState.Open)
				Current.Close();
			Current.Dispose();
		}

		private bool NetworkConnectivityErrorDetectionStrategy(SqlException ex)
		{
			return ex.Number == 11001;
		}

		private sealed class NonQueryResult
		{
			public int RecordsAffected { get; set; }
		}
	}
}