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
			var initiatorIdentifier = _initiatorIdentifier.Current();
			if (initiatorIdentifier != null)
				@event.InitiatorId = initiatorIdentifier.InitiatorId;
		}

		// should use ICurrentDataSource and I...stuff or something
		private void setLogOnInfo(ILogOnInfo message)
		{
			if (_identity == null) return;

			var identity = _identity.Current();
			if (identity == null)
				return;

			message.BusinessUnitId = message.BusinessUnitId.Equals(Guid.Empty)
				? identity.BusinessUnit.Id.GetValueOrDefault()
				: message.BusinessUnitId;
			message.Datasource = identity.DataSource.Application.Name;
		}
	}
}