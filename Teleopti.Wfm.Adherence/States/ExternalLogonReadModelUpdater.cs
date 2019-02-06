using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.States
{
	public class ExternalLogonReadModelUpdater : 
		IRunOnHangfire,
		IHandleEvents,
		IHandleEvent<TenantMinuteTickEvent>
	{
		private readonly IExternalLogonReadModelPersister _persister;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IDistributedLockAcquirer _distributedLock;

		public ExternalLogonReadModelUpdater(
			IExternalLogonReadModelPersister persister, 
			IKeyValueStorePersister keyValueStore,
			IDistributedLockAcquirer distributedLock)
		{
			_persister = persister;
			_keyValueStore = keyValueStore;
			_distributedLock = distributedLock;
		}

		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<PersonAssociationChangedEvent>();
		}

		public void Handle(IEnumerable<IEvent> events)
		{
			events.OfType<PersonAssociationChangedEvent>()
				.ForEach(Add);
		}
		
		[ReadModelUnitOfWork]
		protected virtual void Add(PersonAssociationChangedEvent @event)
		{
			_persister.Delete(@event.PersonId);

			@event.ExternalLogons
				.Select(x => new ExternalLogonReadModel
				{
					PersonId = @event.PersonId,
					UserCode = x.UserCode,
					DataSourceId = x.DataSourceId,
					TimeZone = @event.TimeZone
				})
				.ForEach(model => _persister.Add(model));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				_persister.Refresh();
				_keyValueStore.Update("ExternalLogonReadModelVersion", Guid.NewGuid().ToString());
			});
		}
	}
}