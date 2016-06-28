using System;
using System.Data.SqlClient;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class DistributedLockAcquirer : IDistributedLockAcquirer
	{
		private readonly IConfigReader _configReader;
		private readonly IConnectionStrings _connectionStrings;
		private readonly SqlMonitor _monitor;

		public DistributedLockAcquirer(IConfigReader configReader, IConnectionStrings connectionStrings, SqlMonitor monitor)
		{
			_configReader = configReader;
			_connectionStrings = connectionStrings;
			_monitor = monitor;
		}

		public IDisposable LockForTypeOf(object lockObject)
		{
			var connection = new SqlConnection(_connectionStrings.Application());
			connection.Open();
			var @lock = _monitor.Enter(ProxyUtil.GetUnproxiedType(lockObject).Name, timeout(), connection);
			return new GenericDisposable(() =>
			{
				@lock.Dispose();
				connection.Close();
				connection.Dispose();
			});
		}

		public void TryLockForTypeOf(object lockObject, Action action)
		{
			var connection = new SqlConnection(_connectionStrings.Application());
			connection.Open();
			var resource = ProxyUtil.GetUnproxiedType(lockObject).Name;
			if (_monitor.TryEnter(resource, TimeSpan.Zero, connection))
			{
				action();
				_monitor.Exit(resource, TimeSpan.Zero, connection);
			}
			connection.Close();
			connection.Dispose();
		}

		private TimeSpan timeout()
		{
			return TimeSpan.FromMilliseconds(_configReader.ReadValue("DistributedLockTimeout", 20 * 1000));
		}
	}
}