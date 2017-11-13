using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;

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
		private static ILog Log = LogManager.GetLogger(typeof(AgentStateReadModelMaintainer));

		private readonly IAgentStateReadModelPersister _persister;
		private readonly IJsonSerializer _serializer;

		public AgentStateReadModelMaintainer(IAgentStateReadModelPersister persister, IJsonSerializer serializer)
		{
			_persister = persister;
			_serializer = serializer;
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
			if (@event.PersonId == Guid.Empty)
			{
				Log.Error("PersonNameChangedEvent received was invalid " + _serializer.SerializeObject(@event));
				return;
			}
			if (string.IsNullOrWhiteSpace(@event.FirstName) && string.IsNullOrWhiteSpace(@event.LastName))
			{
				Log.Error("PersonNameChangedEvent received was invalid " + _serializer.SerializeObject(@event));
				return;
			}
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