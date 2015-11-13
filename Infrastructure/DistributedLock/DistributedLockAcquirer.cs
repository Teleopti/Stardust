using System;
using System.Data.SqlClient;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class DistributedLockAcquirer : IDistributedLockAcquirer
	{
		private readonly IConfigReader _configReader;
		private readonly Func<SqlConnection> _connection;

		public DistributedLockAcquirer(IConfigReader configReader, IConnectionStrings connectionStrings)
		{
			_configReader = configReader;
			_connection = () =>
			{
				var conn = new SqlConnection(connectionStrings.Application());
				conn.Open();
				return conn;
			};
		}

		public IDisposable LockForTypeOf(object lockObject)
		{
			var @lock = new SqlServerDistributedLock(ProxyUtil.GetUnproxiedType(lockObject).Name, timeout(), _connection);
			return new GenericDisposable(@lock.Dispose);
		}

		private TimeSpan timeout()
		{
			return TimeSpan.FromMilliseconds(_configReader.ReadValue("DistributedLockTimeout", 20 * 1000));
		}
	}
}