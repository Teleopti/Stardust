using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain;
using log4net;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class SqlMonitor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SqlMonitor));

		public bool TryEnter(string resource, TimeSpan timeout, SqlConnection connection)
		{
			var getCommand = connection.CreateCommand();
			getCommand.CommandType = CommandType.StoredProcedure;
			getCommand.CommandText = @"sp_getapplock";
			getCommand.Parameters.AddWithValue("@Resource", resource);
			getCommand.Parameters.AddWithValue("@DbPrincipal", "public");
			getCommand.Parameters.AddWithValue("@LockMode", "Exclusive");
			getCommand.Parameters.AddWithValue("@LockOwner", "Session");
			getCommand.Parameters.AddWithValue("@LockTimeout", timeout.TotalMilliseconds);
			getCommand.Parameters.Add("@Result", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
			getCommand.ExecuteNonQuery();

			var lockResult = (int)getCommand.Parameters["@Result"].Value;
			logger.DebugFormat("sp_getapplock returned result = {0}", lockResult);

			return lockResult >= 0;
		}

		public void Exit(string resource, TimeSpan timeout, SqlConnection connection)
		{
			var releaseCommand = connection.CreateCommand();
			releaseCommand.CommandType = CommandType.StoredProcedure;
			releaseCommand.CommandText = @"sp_releaseapplock";
			releaseCommand.Parameters.AddWithValue("@Resource", resource);
			releaseCommand.Parameters.AddWithValue("@LockOwner", "Session");
			releaseCommand.Parameters.Add("@Result", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
			releaseCommand.ExecuteNonQuery();

			var releaseResult = (int)releaseCommand.Parameters["@Result"].Value;

			if (releaseResult < 0)
				throw new DistributedLockException(string.Format("Could not release a lock on the resource " + resource));
		}

		public IDisposable Enter(string resource, TimeSpan timeout, SqlConnection connection)
		{
			if (!TryEnter(resource, timeout, connection))
				throw new DistributedLockException(string.Format("Could not place a lock on the resource " + resource));
			return new GenericDisposable(() =>
			{
				Exit(resource, timeout, connection);
			});
		}
		
	}
}