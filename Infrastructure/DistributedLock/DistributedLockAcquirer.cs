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
			return @lock(ProxyUtil.GetUnproxiedType(lockObject).Name);
		}

		public IDisposable LockForGuid(object lockObject, Guid guid)
		{
			return @lock($"{ProxyUtil.GetUnproxiedType(lockObject).Name}{guid}");
		}

		

		public void TryLockForTypeOf(object lockObject, Action action)
		{
			tryLock(ProxyUtil.GetUnproxiedType(lockObject).Name, action);
		}

		public void TryLockForGuid(object lockObject, Guid guid, Action action)
		{
			tryLock($"{ProxyUtil.GetUnproxiedType(lockObject).Name}{guid}", action);
		}

		private void tryLock(string name, Action action)
		{
			using (var connection = new SqlConnection(_connectionStrings.Application()))
			{
				connection.Open();
				if (_monitor.TryEnter(name, TimeSpan.Zero, connection))
				{
					try
					{
						action();
					}
					finally
					{
						_monitor.Exit(name, TimeSpan.Zero, connection);
					}
				}
				connection.Close();
			}
		}

		private IDisposable @lock(string name)
		{
			var connection = new SqlConnection(_connectionStrings.Application());
			connection.Open();
			var @lock = _monitor.Enter(name, timeout(), connection);
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