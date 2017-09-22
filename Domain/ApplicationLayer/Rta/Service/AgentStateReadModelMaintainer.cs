using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelMaintainer :
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<PersonNameChangedEvent>,
		IHandleEvent<PersonEmploymentNumberChangedEvent>,
		IHandleEvent<SiteNameChangedEvent>,
		IHandleEvent<TeamNameChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAgentStateReadModelPersister _persister;

		public AgentStateReadModelMaintainer(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}
		
		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (!@event.TeamId.HasValue)
			{
				_persister.UpsertDeleted(@event.PersonId);
				return;
			}
			if (@event.ExternalLogons.IsNullOrEmpty())
			{
				_persister.UpsertDeleted(@event.PersonId);
				return;
			}
			_persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = @event.PersonId,
				BusinessUnitId = @event.BusinessUnitId,
				SiteId = @event.SiteId,
				SiteName = @event.SiteName,
				TeamId = @event.TeamId.Value,
				TeamName = @event.TeamName
			});
		}

		[UnitOfWork]
		public virtual void Handle(PersonNameChangedEvent @event)
		{
			_persister.UpsertName(@event.PersonId, @event.FirstName, @event.LastName);
		}

		[UnitOfWork]
		public virtual void Handle(PersonEmploymentNumberChangedEvent @event)
		{
			_persister.UpsertEmploymentNumber(@event.PersonId, @event.EmploymentNumber);
		}

		[UnitOfWork]
		public virtual void Handle(TeamNameChangedEvent @event)
		{
			_persister.UpdateTeamName(@event.TeamId, @event.Name);
		}

		[UnitOfWork]
		public virtual void Handle(SiteNameChangedEvent @event)
		{
			_persister.UpdateSiteName(@event.SiteId, @event.Name);
		}
		
		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.UpsertDeleted(@event.PersonId);
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