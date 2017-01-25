using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<TenantHourTickEvent>
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly INow _now;

		public AgentStateReadModelMaintainer(IAgentStateReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		[UnitOfWork]
		public virtual void Handle(TenantHourTickEvent @event)
		{
			_persister.DeleteOldRows(_now.UtcDateTime());
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.SetDeleted(@event.PersonId, expirationFor(@event));
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{

			if (!@event.TeamId.HasValue)
			{
				_persister.SetDeleted(@event.PersonId, expirationFor(@event));
				return;
			}
			if (@event.ExternalLogons.IsNullOrEmpty())
			{
				_persister.SetDeleted(@event.PersonId, expirationFor(@event));
				return;
			}
			var model = _persister.Load(@event.PersonId);
			if (model == null || expirationFor(@event) >= model.ExpiresAt.GetValueOrDefault())
				_persister.UpsertAssociation(new AssociationInfo()
				{
					PersonId = @event.PersonId,
					BusinessUnitId = @event.BusinessUnitId,
					SiteId = @event.SiteId,
					SiteName = @event.SiteName,
					TeamId = @event.TeamId.Value,
					TeamName = @event.TeamName
				});
		}

		private static DateTime expirationFor(IEvent @event)
		{
			return ((dynamic) @event).Timestamp.AddMinutes(30);
		}

	}

	public class AssociationInfo
	{
		public Guid PersonId { get; set; }
		public Guid? BusinessUnitId { get; set; }
		public Guid? SiteId { get; set; }
		public string SiteName { get; set; }
		public Guid TeamId { get; set; }
		public string TeamName { get; set; }
	}
}