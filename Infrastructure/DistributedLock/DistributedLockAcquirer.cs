using System;
using System.Data.SqlClient;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			var connection = new SqlConnection(_configReader.ConnectionStrings["RtaApplication"].ConnectionString);
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
			if (_configReader.AppSettings["DistributedLockTimeout"] == null)
				return TimeSpan.FromSeconds(20);
			return TimeSpan.FromMilliseconds(int.Parse(_configReader.AppSettings["DistributedLockTimeout"]));
		}
	}
}