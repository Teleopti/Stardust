using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class ReliableSqlDbConnection : DbConnection
	{
		private bool disposed;

		/// <summary>
		/// The underlying <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		public ReliableSqlConnection ReliableConnection { get; set; }

		/// <summary>
		/// Constructs a <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> to wrap around the given <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		/// <param name="connection">The <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" /> to wrap</param>
		public ReliableSqlDbConnection(ReliableSqlConnection connection)
		{
			this.ReliableConnection = connection;
		}

		/// <summary>
		/// Explicit type-casting between <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> and <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		/// <param name="connection">The <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> being casted</param>
		/// <returns>The underlying <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" /></returns>
		public static explicit operator SqlConnection(ReliableSqlDbConnection connection)
		{
			return connection.ReliableConnection.Current;
		}

		protected override void Dispose(bool disposing)
		{
			if (this.disposed)
				return;
			if (disposing)
				this.ReliableConnection.Dispose();
			this.disposed = true;
			base.Dispose(disposing);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return (DbTransaction)this.ReliableConnection.BeginTransaction(isolationLevel);
		}

		public override void Close()
		{
			this.ReliableConnection.Close();
		}

		public override DataTable GetSchema()
		{
			return this.ReliableConnection.ConnectionRetryPolicy.ExecuteAction<DataTable>((Func<DataTable>)(() => this.ReliableConnection.Current.GetSchema()));
		}

		public override DataTable GetSchema(string collectionName)
		{
			return this.ReliableConnection.ConnectionRetryPolicy.ExecuteAction<DataTable>((Func<DataTable>)(() => this.ReliableConnection.Current.GetSchema(collectionName)));
		}

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			return this.ReliableConnection.ConnectionRetryPolicy.ExecuteAction<DataTable>((Func<DataTable>)(() => this.ReliableConnection.Current.GetSchema(collectionName, restrictionValues)));
		}

		public override void ChangeDatabase(string databaseName)
		{
			this.ReliableConnection.ChangeDatabase(databaseName);
		}

		protected override DbCommand CreateDbCommand()
		{
			return (DbCommand)this.ReliableConnection.CreateCommand();
		}

		public override void Open()
		{
			this.ReliableConnection.Open();
		}

		public override string ConnectionString
		{
			get
			{
				return this.ReliableConnection.ConnectionString;
			}
			set
			{
				this.ReliableConnection.ConnectionString = value;
			}
		}

		public override int ConnectionTimeout
		{
			get
			{
				return this.ReliableConnection.ConnectionTimeout;
			}
		}

		public override string Database
		{
			get
			{
				return this.ReliableConnection.Database;
			}
		}

		public override string DataSource
		{
			get
			{
				return "";
			}
		}

		public override string ServerVersion
		{
			get
			{
				return "";
			}
		}

		public override ConnectionState State
		{
			get
			{
				return this.ReliableConnection.State;
			}
		}
	}
}