using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
{
	public class AgentStateReadModelMaintainer :
		IHandleEvent<PersonAssociationChangedEvent>,
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
				_persister.UpsertNoAssociation(@event.PersonId);
				return;
			}

			if (@event.ExternalLogons.IsNullOrEmpty())
			{
				_persister.UpsertNoAssociation(@event.PersonId);
				return;
			}

			_persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = @event.PersonId,
				BusinessUnitId = @event.BusinessUnitId,
				SiteId = @event.SiteId,
				SiteName = @event.SiteName,
				TeamId = @event.TeamId.Value,
				TeamName = @event.TeamName,
				FirstName = @event.FirstName,
				LastName = @event.LastName,
				EmploymentNumber = @event.EmploymentNumber,
			});
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
	}
}