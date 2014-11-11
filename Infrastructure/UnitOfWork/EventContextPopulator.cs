using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventContextPopulator : IEventContextPopulator
	{
		private readonly ICurrentIdentity _identity;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public EventContextPopulator(ICurrentIdentity identity, ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_identity = identity;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void PopulateEventContext(IEvent @event)
		{
			var domainEvents = @event as ILogOnInfo;
			if (domainEvents == null) return;
			
			setInitiator(domainEvents);
			PopulateEventContext(domainEvents);
		}


		public void PopulateEventContext(ILogOnInfo @event)
		{
			setInitiator(@event);
			setValuesFromIdentity(@event);
		}

		private void setInitiator(ILogOnInfo domainEvents)
		{
			var initiatorIdentifier = _initiatorIdentifier.Current();
			if (initiatorIdentifier != null)
				domainEvents.InitiatorId = initiatorIdentifier.InitiatorId;
		}

		public void PopulateEventContextWithoutInitiator(ILogOnInfo message)
		{
			setValuesFromIdentity(message);
		}

		private void setValuesFromIdentity(ILogOnInfo message)
		{
			if (_identity == null) return;

			var identity = _identity.Current();
			message.BusinessUnitId = message.BusinessUnitId.Equals(Guid.Empty)
				? identity.BusinessUnit.Id.GetValueOrDefault()
				: message.BusinessUnitId;
			message.Datasource = identity.DataSource.Application.Name;
		}
	}
}