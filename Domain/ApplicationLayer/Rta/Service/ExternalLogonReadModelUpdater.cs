using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ExternalLogonReadModelUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<TenantMinuteTickEvent>
	{
		private readonly IExternalLogonReadModelPersister _persister;
		private readonly IKeyValueStorePersister _keyValueStore;

		public ExternalLogonReadModelUpdater(IExternalLogonReadModelPersister persister, IKeyValueStorePersister keyValueStore)
		{
			_persister = persister;
			_keyValueStore = keyValueStore;
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
			_persister.Refresh();
			_keyValueStore.Update("ExternalLogonReadModelVersion", Guid.NewGuid().ToString());
		}
	}
}