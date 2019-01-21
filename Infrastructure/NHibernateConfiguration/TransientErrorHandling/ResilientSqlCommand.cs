using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	/// <summary>
	/// An <see cref="IDbCommand"/> implementation that wraps a <see cref="SqlCommand"/> object such that any
	/// queries that are executed are executed via a ReliableSqlConnection.
	/// </summary>
	/// <remarks>
	/// Note: For this to work it requires that the Connection property be set with a ReliableSqlConnection object.
	/// </remarks>
	public class ResilientSqlCommand : DbCommand
	{
		/// <summary>
		/// The underlying <see cref="SqlCommand"/> being proxied.
		/// </summary>
		public System.Data.SqlClient.SqlCommand Current { get; private set; }

		/// <summary>
		/// The <see cref="ResilientSqlConnection"/> that has been assigned to the command via the Connection property.
		/// </summary>
		public ResilientSqlConnection ReliableConnection { get; set; }

		/// <summary>
		/// Constructs a <see cref="ResilientSqlCommand"/>.
		/// </summary>
		public ResilientSqlCommand()
		{
			Current = new System.Data.SqlClient.SqlCommand();
		}

		/// <summary>
		/// Explicit type-casting between a <see cref="ResilientSqlCommand"/> and a <see cref="SqlCommand"/>.
		/// </summary>
		/// <param name="command">The <see cref="ResilientSqlCommand"/> being casted</param>
		/// <returns>The underlying <see cref="SqlCommand"/> being proxied.</returns>
		public static explicit operator System.Data.SqlClient.SqlCommand(ResilientSqlCommand command)
		{
			return command.Current;
		}

		/// <summary>
		/// Returns the underlying <see cref="SqlConnection"/> and expects a <see cref="ResilientSqlConnection"/> when being set.
		/// </summary>
		public new DbConnection Connection
		{
			get { return Current.Connection; }
			set
			{
				ReliableConnection = ((ResilientSqlDbConnection)value).ReliableConnection;
				Current.Connection = ReliableConnection.Current;
			}
		}

		protected override DbConnection DbConnection
		{
			get => Connection;
			set => Connection = value;
		}

		protected override DbParameterCollection DbParameterCollection => Parameters;

		protected override DbTransaction DbTransaction
		{
			get => Transaction;
			set => Transaction = value;
		}

		public override bool DesignTimeVisible
		{
			get => Current.DesignTimeVisible;
			set => Current.DesignTimeVisible = value;
		}

		#region Wrapping code

		public new void Dispose()
		{
			Current.Dispose();
		}

		public override void Prepare()
		{
			Current.Prepare();
		}

		public override void Cancel()
		{
			Current.Cancel();
		}

		public new IDbDataParameter CreateParameter()
		{
			return Current.CreateParameter();
		}

		protected override DbParameter CreateDbParameter()
		{
			return Current.CreateParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return Current.ExecuteReader(behavior);
		}

		public override int ExecuteNonQuery()
		{
			return ReliableConnection.ExecuteCommand(Current);
		}

		public new IDataReader ExecuteReader()
		{
			return ReliableConnection.ExecuteCommand<IDataReader>(Current);
		}

		public new IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return ReliableConnection.ExecuteCommand<IDataReader>(Current, behavior);
		}

		public override object ExecuteScalar()
		{
			return ReliableConnection.ExecuteCommand<int>(Current);
		}

		public new DbTransaction Transaction
		{
			get { return Current.Transaction; }
			set { Current.Transaction = (SqlTransaction)value; }
		}

		public override string CommandText
		{
			get { return Current.CommandText; }
			set { Current.CommandText = value; }
		}

		public override int CommandTimeout
		{
			get { return Current.CommandTimeout; }
			set { Current.CommandTimeout = value; }
		}

		public override CommandType CommandType
		{
			get { return Current.CommandType; }
			set { Current.CommandType = value; }
		}

		public new DbParameterCollection Parameters
		{
			get { return Current.Parameters; }
		}

		public override UpdateRowSource UpdatedRowSource
		{
			get { return Current.UpdatedRowSource; }
			set { Current.UpdatedRowSource = value; }
		}

		#endregion
	}
}