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
		private readonly IConnectionStrings _connectionStrings;

		public DistributedLockAcquirer(IConfigReader configReader, IConnectionStrings connectionStrings)
		{
			_configReader = configReader;
			_connectionStrings = connectionStrings;
		}

		public IDisposable LockForTypeOf(object lockObject)
		{
			var connection = new SqlConnection(_connectionStrings.Application());
			connection.Open();
			var @lock = new SqlServerDistributedLock(ProxyUtil.GetUnproxiedType(lockObject).Name, timeout(), connection);
			return new GenericDisposable(() =>
			{
				@lock.Dispose();
				connection.Close();
				connection.Dispose();
			});
		}

		private TimeSpan timeout()
		{
			return TimeSpan.FromMilliseconds(_configReader.ReadValue("DistributedLockTimeout", 20 * 1000));
		}
	}
}