using System;
using System.Linq;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence
{
	public interface IRtaEventPublisher : IEventPublisher
	{
	}

	[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	public class NoRtaEventPublisher : IRtaEventPublisher
	{
		public void Publish(params IEvent[] events)
		{
		}
	}

	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IRtaEventStore _store;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ISyncEventProcessingExceptionHandler _exceptionHandler;

		public RtaEventPublisher(
			IRtaEventStore store,
			ICurrentDataSource currentDataSource,
			ISyncEventProcessingExceptionHandler exceptionHandler)
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

			var dataSource = _currentDataSource.CurrentName();
			var thread = new Thread(() =>
			{
				try
				{
					withUnitOfWork(dataSource, () => storedEvents.ForEach(_store.Add));
				}
				catch (Exception e)
				{
					_exceptionHandler.Handle(e);
				}
			});
			thread.Start();
			thread.Join();
		}

		[TenantScope]
		[UnitOfWork]
		protected virtual void withUnitOfWork(string tenant, Action action)
		{
			action();
		}
	}
}