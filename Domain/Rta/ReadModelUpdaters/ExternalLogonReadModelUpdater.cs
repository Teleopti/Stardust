using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Rta.Service;

namespace Teleopti.Ccc.Domain.Rta.ReadModelUpdaters
{
	public class ExternalLogonReadModelUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonAssociationChangedEvent>,
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

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			_persister.Delete(@event.PersonId);

			@event.ExternalLogons
				.Select(x => new ExternalLogonReadModel
				{
					PersonId = @event.PersonId,
					UserCode = x.UserCode,
					DataSourceId = x.DataSourceId
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