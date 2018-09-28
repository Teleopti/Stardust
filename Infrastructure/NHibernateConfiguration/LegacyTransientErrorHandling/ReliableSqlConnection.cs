using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public sealed class ReliableSqlConnection : IDbConnection, IDisposable, ICloneable
	{
		private readonly SqlConnection underlyingConnection;
		private readonly RetryPolicy connectionRetryPolicy;
		private readonly RetryPolicy commandRetryPolicy;
		private readonly RetryPolicy connectionStringFailoverPolicy;
		private string connectionString;

		public ReliableSqlConnection(string connectionString)
			: this(connectionString, RetryManager.Instance.GetDefaultSqlConnectionRetryPolicy())
		{
		}

		public ReliableSqlConnection(string connectionString, RetryPolicy retryPolicy)
			: this(connectionString, retryPolicy, RetryManager.Instance.GetDefaultSqlCommandRetryPolicy() ?? retryPolicy)
		{
		}

		public ReliableSqlConnection(string connectionString, RetryPolicy connectionRetryPolicy, RetryPolicy commandRetryPolicy)
		{
			this.connectionString = connectionString;
			this.underlyingConnection = new SqlConnection(connectionString);
			this.connectionRetryPolicy = connectionRetryPolicy;
			this.commandRetryPolicy = commandRetryPolicy;
			this.connectionStringFailoverPolicy = (RetryPolicy)new RetryPolicy<ReliableSqlConnection.NetworkConnectivityErrorDetectionStrategy>(1, TimeSpan.FromMilliseconds(1.0));
		}

		public string ConnectionString
		{
			get
			{
				return this.connectionString;
			}
			set
			{
				this.connectionString = value;
				this.underlyingConnection.ConnectionString = value;
			}
		}

		public RetryPolicy ConnectionRetryPolicy
		{
			get
			{
				return this.connectionRetryPolicy;
			}
		}

		public RetryPolicy CommandRetryPolicy
		{
			get
			{
				return this.commandRetryPolicy;
			}
		}

		public SqlConnection Current
		{
			get
			{
				return this.underlyingConnection;
			}
		}

		public Guid SessionTracingId
		{
			get
			{
				try
				{
					using (IDbCommand contextInfoCommand = SqlCommandFactory.CreateGetContextInfoCommand((IDbConnection)this.Current))
						return this.ExecuteCommand<Guid>(contextInfoCommand);
				}
				catch
				{
					return Guid.Empty;
				}
			}
		}

		public int ConnectionTimeout
		{
			get
			{
				return this.underlyingConnection.ConnectionTimeout;
			}
		}

		public string Database
		{
			get
			{
				return this.underlyingConnection.Database;
			}
		}

		public ConnectionState State
		{
			get
			{
				return this.underlyingConnection.State;
			}
		}

		public SqlConnection Open()
		{
			return this.Open(this.ConnectionRetryPolicy);
		}

		public SqlConnection Open(RetryPolicy retryPolicy)
		{
			(retryPolicy ?? RetryPolicy.NoRetry).ExecuteAction((Action)(() => this.connectionStringFailoverPolicy.ExecuteAction((Action)(() =>
			{
				if (this.underlyingConnection.State == ConnectionState.Open)
					return;
				this.underlyingConnection.Open();
			}))));
			return this.underlyingConnection;
		}

		public T ExecuteCommand<T>(IDbCommand command)
		{
			return this.ExecuteCommand<T>(command, this.CommandRetryPolicy, CommandBehavior.Default);
		}

		public T ExecuteCommand<T>(IDbCommand command, CommandBehavior behavior)
		{
			return this.ExecuteCommand<T>(command, this.CommandRetryPolicy, behavior);
		}

		public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy)
		{
			return this.ExecuteCommand<T>(command, retryPolicy, CommandBehavior.Default);
		}

		public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy, CommandBehavior behavior)
		{
			T actionResult = default(T);
			Type resultType = typeof(T);
			bool hasOpenedConnection = false;
			bool closeOpenedConnectionOnSuccess = false;
			try
			{
				(retryPolicy ?? RetryPolicy.NoRetry).ExecuteAction((Action)(() => actionResult = this.connectionStringFailoverPolicy.ExecuteAction<T>((Func<T>)(() =>
				{
					if (command.Connection == null)
					{
						command.Connection = (IDbConnection)this.Open();
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
						return (T)command.ExecuteReader(behavior);
					}
					if (resultType == typeof(XmlReader))
					{
						if (!(command is SqlCommand sqlCommand))
							throw new NotSupportedException();
						XmlReader innerReader = sqlCommand.ExecuteXmlReader();
						closeOpenedConnectionOnSuccess = false;
						return (T)((behavior & CommandBehavior.CloseConnection) != CommandBehavior.CloseConnection ? (object)innerReader : (object)new SqlXmlReader(command.Connection, innerReader));
					}
					if (resultType == typeof(ReliableSqlConnection.NonQueryResult))
					{
						ReliableSqlConnection.NonQueryResult nonQueryResult = new ReliableSqlConnection.NonQueryResult()
						{
							RecordsAffected = command.ExecuteNonQuery()
						};
						closeOpenedConnectionOnSuccess = true;
						return (T)Convert.ChangeType((object)nonQueryResult, resultType, (IFormatProvider)CultureInfo.InvariantCulture);
					}
					object obj = command.ExecuteScalar();
					closeOpenedConnectionOnSuccess = true;
					if (obj != null)
						return (T)Convert.ChangeType(obj, resultType, (IFormatProvider)CultureInfo.InvariantCulture);
					return default(T);
				}))));
				if (hasOpenedConnection)
				{
					if (closeOpenedConnectionOnSuccess)
					{
						if (command.Connection != null)
						{
							if (command.Connection.State == ConnectionState.Open)
								command.Connection.Close();
						}
					}
				}
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
			return this.ExecuteCommand(command, this.CommandRetryPolicy);
		}

		public int ExecuteCommand(IDbCommand command, RetryPolicy retryPolicy)
		{
			return this.ExecuteCommand<ReliableSqlConnection.NonQueryResult>(command, retryPolicy).RecordsAffected;
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return (IDbTransaction)this.underlyingConnection.BeginTransaction(il);
		}

		public IDbTransaction BeginTransaction()
		{
			return (IDbTransaction)this.underlyingConnection.BeginTransaction();
		}

		public void ChangeDatabase(string databaseName)
		{
			this.underlyingConnection.ChangeDatabase(databaseName);
		}

		void IDbConnection.Open()
		{
			this.Open();
		}

		public void Close()
		{
			this.underlyingConnection.Close();
		}

		public SqlCommand CreateCommand()
		{
			return this.underlyingConnection.CreateCommand();
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return (IDbCommand)this.underlyingConnection.CreateCommand();
		}

		object ICloneable.Clone()
		{
			return (object)new ReliableSqlConnection(this.ConnectionString, this.ConnectionRetryPolicy, this.CommandRetryPolicy);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize((object)this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing)
				return;
			if (this.underlyingConnection.State == ConnectionState.Open)
				this.underlyingConnection.Close();
			this.underlyingConnection.Dispose();
		}

		private sealed class NonQueryResult
		{
			public int RecordsAffected { get; set; }
		}

		private sealed class NetworkConnectivityErrorDetectionStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				SqlException sqlException;
				return ex != null && (sqlException = ex as SqlException) != null && sqlException.Number == 11001;
			}
		}
	}
}