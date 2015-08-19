using System;
using System.Data.SqlClient;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class DistributedLockAcquirer : IDistributedLockAcquirer
	{
		private readonly IConfigReader _configReader;

		public DistributedLockAcquirer(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public IDisposable LockForTypeOf(object lockObject)
		{
			var connection = new SqlConnection(_configReader.ConnectionString("RtaApplication"));
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