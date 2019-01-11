using System;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IRtaEventStore _store;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IRtaEventPublisherExceptionHandler _exceptionHandler;

		public RtaEventPublisher(
			IRtaEventStore store,
			ICurrentDataSource currentDataSource,
			IRtaEventPublisherExceptionHandler exceptionHandler)
		{
			_exceptionHandler = exceptionHandler;
			_store = store;
			_currentDataSource = currentDataSource;
		}

		public void Publish(params IEvent[] events)
		{
			var storedEvents = events.OfType<IRtaStoredEvent>().Cast<IEvent>().ToArray();
			if (!storedEvents.Any())
				return;

			Exception exception = null;
			var dataSource = _currentDataSource.CurrentName();
			var thread = new Thread(() =>
			{
				try
				{
					withUnitOfWork(dataSource, () => _store.Add(storedEvents, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion));
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
			thread.Start();
			thread.Join();
			
			if (exception != null)
				_exceptionHandler.Handle(exception);
		}

		[TenantScope]
		[UnitOfWork]
		protected virtual void withUnitOfWork(string tenant, Action action)
		{
			action();
		}
	}
}