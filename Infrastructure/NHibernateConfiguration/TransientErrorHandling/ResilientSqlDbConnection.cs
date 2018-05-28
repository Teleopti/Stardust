using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public class ResilientSqlDbConnection : DbConnection
	{
		private bool disposed;

		/// <summary>
		/// The underlying <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		public ResilientSqlConnection ReliableConnection { get; set; }

		/// <summary>
		/// Constructs a <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> to wrap around the given <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		/// <param name="connection">The <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" /> to wrap</param>
		public ResilientSqlDbConnection(ResilientSqlConnection connection)
		{
			ReliableConnection = connection;
		}

		/// <summary>
		/// Explicit type-casting between <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> and <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" />.
		/// </summary>
		/// <param name="connection">The <see cref="T:NHibernate.SqlAzure.ReliableSqlDbConnection" /> being casted</param>
		/// <returns>The underlying <see cref="T:Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection" /></returns>
		public static explicit operator SqlConnection(ResilientSqlDbConnection connection)
		{
			return connection.ReliableConnection.Current;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing)
				ReliableConnection.Dispose();
			disposed = true;
			base.Dispose(disposing);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return (DbTransaction)ReliableConnection.BeginTransaction(isolationLevel);
		}

		public override void Close()
		{
			ReliableConnection.Close();
		}

		public override DataTable GetSchema()
		{
			return ReliableConnection.ConnectionRetryPolicy.Execute(() => ReliableConnection.Current.GetSchema());
		}

		public override DataTable GetSchema(string collectionName)
		{
			return ReliableConnection.ConnectionRetryPolicy.Execute(() => ReliableConnection.Current.GetSchema(collectionName));
		}

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			return ReliableConnection.ConnectionRetryPolicy.Execute(() => ReliableConnection.Current.GetSchema(collectionName, restrictionValues));
		}

		public override void ChangeDatabase(string databaseName)
		{
			ReliableConnection.ChangeDatabase(databaseName);
		}

		protected override DbCommand CreateDbCommand()
		{
			return ReliableConnection.CreateCommand();
		}

		public override void Open()
		{
			ReliableConnection.Open();
		}

		public override string ConnectionString
		{
			get => ReliableConnection.ConnectionString;
			set => ReliableConnection.ConnectionString = value;
		}

		public override int ConnectionTimeout => ReliableConnection.ConnectionTimeout;

		public override string Database => ReliableConnection.Database;

		public override string DataSource => "";

		public override string ServerVersion => "";

		public override ConnectionState State => ReliableConnection.State;
	}
}