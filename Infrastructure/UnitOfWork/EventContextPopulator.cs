using System;
using Teleopti.Ccc.Domain.Common;
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

		public void PopulateEventContext(object @event)
		{
			var initiatorInfo = @event as IInitiatorInfo;
			if (initiatorInfo != null)
				setInitiator(initiatorInfo);
			var logOnInfo = @event as ILogOnInfo;
			if (logOnInfo != null)
				setLogOnInfo(logOnInfo);
		}

		private void setInitiator(IInitiatorInfo @event)
		{
			if (@event.InitiatorId != Guid.Empty)
				return;
			var initiatorIdentifier = _initiatorIdentifier.Current();
			if (initiatorIdentifier != null)
				@event.InitiatorId = initiatorIdentifier.InitiatorId;
		}

		private void setLogOnInfo(ILogOnInfo @event)
		{
			if (!string.IsNullOrEmpty(@event.Datasource))
				return;

			if (_identity == null) return;

			var identity = _identity.Current();
			if (identity == null)
				return;

			@event.BusinessUnitId = @event.BusinessUnitId.Equals(Guid.Empty)
				? identity.BusinessUnit.Id.GetValueOrDefault()
				: @event.BusinessUnitId;
			@event.Datasource = identity.DataSource.Application.Name;
		}
	}
}