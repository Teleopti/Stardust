using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class SqlServerDistributedLock : IDisposable
	{
		private readonly Func<SqlConnection> _connection;
		private readonly string _resource;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();

		public SqlServerDistributedLock(string resource, TimeSpan timeout, Func<SqlConnection> connection)
		{
			_resource = resource;
			_connection = connection;

			_executor.Run(_connection, conn =>
			{
				var command = conn.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"sp_getapplock";
				command.Parameters.AddWithValue("@Resource", _resource);
				command.Parameters.AddWithValue("@DbPrincipal", "public");
				command.Parameters.AddWithValue("@LockMode", "Exclusive");
				command.Parameters.AddWithValue("@LockOwner", "Session");
				command.Parameters.AddWithValue("@LockTimeout", timeout.TotalMilliseconds);
				command.Parameters.Add("@Result", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
				command.ExecuteNonQuery();

				var lockResult = (int) command.Parameters["@Result"].Value;

				if (lockResult < 0)
					throw new DistributedLockException(string.Format("Could not place a lock on the resource " + _resource));
			});
		}

		public void Dispose()
		{
			_executor.Run(_connection, conn =>
			{
				var command = conn.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"sp_releaseapplock";
				command.Parameters.AddWithValue("@Resource", _resource);
				command.Parameters.AddWithValue("@LockOwner", "Session");
				command.Parameters.Add("@Result", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
				command.ExecuteNonQuery();

				var releaseResult = (int) command.Parameters["@Result"].Value;

				if (releaseResult < 0)
					throw new DistributedLockException(string.Format("Could not release a lock on the resource " + _resource));
			});
		}
	}
}