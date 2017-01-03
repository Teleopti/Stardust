using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class DistributedLockAcquirer : IDistributedLockAcquirer
	{
		private readonly IConnectionStrings _connectionStrings;
		private readonly SqlMonitor _monitor;
		private readonly TimeSpan _timeout;
		private readonly TimeSpan _keepAliveInterval;

		public DistributedLockAcquirer(
			IConfigReader config, 
			IConnectionStrings connectionStrings, 
			SqlMonitor monitor)
		{
			_timeout = TimeSpan.FromMilliseconds(config.ReadValue("DistributedLockTimeout", 20*1000));
			_keepAliveInterval = TimeSpan.FromMilliseconds(config.ReadValue("DistributedLockKeepAliveInterval", 60*1000));
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
			using (var connection = new SqlConnection(connectionString()))
			{
				connection.Open();
				if (_monitor.TryEnter(name, TimeSpan.Zero, connection))
				{
					var timer = new Timer(_ => keepAlive(connection), null, _keepAliveInterval, _keepAliveInterval);
					try
					{
						action();
					}
					finally
					{
						_monitor.Exit(name, TimeSpan.Zero, connection);
						timer.Dispose();
					}
				}
				connection.Close();
			}
		}

		private IDisposable @lock(string name)
		{
			var connection = new SqlConnection(connectionString());
			connection.Open();
			try
			{
				var @lock = _monitor.Enter(name, _timeout, connection);
				var timer = new Timer(_ => keepAlive(connection), null, _keepAliveInterval, _keepAliveInterval);
				return new GenericDisposable(() =>
				{
					try
					{
						@lock.Dispose();
						connection.Close();
					}
					finally
					{
						timer.Dispose();
						connection.Dispose();
					}
				});
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		// Connections to SQL Azure Database that are idle for 30 minutes 
		// or longer will be terminated. And since we are using separate
		// connection for a distributed lock, we'd like to prevent Resource
		// Governor from terminating it.
		private void keepAlive(IDbConnection connection)
		{
			try
			{
				var command = connection.CreateCommand();
				command.CommandText = @"SELECT 1";
				command.ExecuteNonQuery();
			}
			catch
			{
				// Connection is broken. This means that distributed lock
				// was released, and we can't guarantee the safety property
				// for the code that is wrapped with this block. So it was
				// a bad idea to have a separate connection for just
				// distributed lock.
				// TODO: Think about distributed locks and connections.
			}
		}

		private string connectionString()
		{
			return new SqlConnectionStringBuilder(_connectionStrings.Application())
			{
				ApplicationName = "Teleopti.DistributedLock"
			}.ConnectionString;
		}
		
	}
}