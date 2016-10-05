using System;
using System.Security.Cryptography.X509Certificates;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly INow _now;

		public AgentStateReadModelMaintainer(IAgentStateReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.SetDeleted(@event.PersonId, expirationFor(@event));
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			_persister.DeleteOldRows(_now.UtcDateTime());

			if (@event.TeamId.HasValue)
			{
				var model = _persister.Get(@event.PersonId);
				if (model == null || expirationFor(@event) >= model.ExpiresAt.GetValueOrDefault())
					_persister.UpsertAssociation(@event.PersonId, @event.TeamId.Value, @event.SiteId, @event.BusinessUnitId);
			}
			
			else
				_persister.SetDeleted(@event.PersonId, expirationFor(@event));
		}

		private static DateTime expirationFor(IEvent @event)
		{
			return ((dynamic) @event).Timestamp.AddMinutes(30);
		}
	}
}