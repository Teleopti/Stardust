﻿using System;
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
			var domainEvents = @event as IRaptorDomainMessageInfo;
			if (domainEvents != null)
				PopulateEventContext(domainEvents);
		}

		public void PopulateEventContext(IRaptorDomainMessageInfo @event)
		{
			setValuesFromIdentity(@event);
		}

		private void setValuesFromIdentity(IRaptorDomainMessageInfo message)
		{
			message.Timestamp = DateTime.UtcNow;
			var initiatorIdentifier = _initiatorIdentifier.Current();
			if (initiatorIdentifier != null)
				message.InitiatorId = initiatorIdentifier.InitiatorId;
			if (_identity != null)
			{
				var identity = _identity.Current();
				message.BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
				message.Datasource = identity.DataSource.Application.Name;
			}
		}
	}
}