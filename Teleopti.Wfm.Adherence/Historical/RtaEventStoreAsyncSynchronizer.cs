using System;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Wfm.Adherence.Historical
{
	public interface IRtaEventStoreAsyncSynchronizer
	{
		void SynchronizeAsync();
	}

	public class RtaEventStoreAsyncSynchronizer : IRtaEventStoreAsyncSynchronizer
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly IRtaEventStoreAsyncSynchronizerStrategy _strategy;

		public RtaEventStoreAsyncSynchronizer(
			ICurrentDataSource dataSource,
			IRtaEventStoreSynchronizer synchronizer,
			IRtaEventStoreAsyncSynchronizerStrategy strategy)
		{
			_dataSource = dataSource;
			_synchronizer = synchronizer;
			_strategy = strategy;
		}

		public void SynchronizeAsync()
		{
			Exception exception = null;
			var tenant = _dataSource.CurrentName();
			var thread = new Thread(() =>
			{
				try
				{
					WithTenant(tenant, () => _synchronizer.Synchronize());
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
			thread.Start();
			_strategy.MayWait(thread);
			if (exception != null)
				_strategy.Handle(exception);
		}

		[TenantScope]
		protected virtual void WithTenant(string tenant, Action action) => action();
	}

	public interface IRtaEventStoreAsyncSynchronizerStrategy
	{
		void MayWait(Thread thread);
		void Handle(Exception exception);
	}

	public class RunAsynchronouslyAndLog : IRtaEventStoreAsyncSynchronizerStrategy
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(RunAsynchronouslyAndLog));

		public void MayWait(Thread thread)
		{
		}

		public void Handle(Exception exception)
		{
			_logger.Warn("Triggered events synchronizing failed, will run at a later time.", exception);
		}
	}

	public class RunSynchronouslyAndThrow : IRtaEventStoreAsyncSynchronizerStrategy
	{
		public void MayWait(Thread thread)
		{
			thread.Join();
		}

		public void Handle(Exception exception)
		{
			throw exception;
		}
	}
}