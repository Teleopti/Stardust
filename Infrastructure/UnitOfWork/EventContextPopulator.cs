using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventContextPopulator : IEventContextPopulator
	{
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public static IEventContextPopulator Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal());
			return new EventContextPopulator(
				CurrentBusinessUnit.Make(),
				new CurrentDataSource(identity, new DataSourceState()),
				new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())
				);
		}

		public EventContextPopulator(ICurrentBusinessUnit businessUnit, ICurrentDataSource dataSource, ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_businessUnit = businessUnit;
			_dataSource = dataSource;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void PopulateEventContext(params object[] events)
		{
			foreach (var @event in events)
			{
				var initiatorInfo = @event as IInitiatorInfo;
				if (initiatorInfo != null)
					setInitiator(initiatorInfo);
				var logOnInfo = @event as ILogOnInfo;
				if (logOnInfo != null)
					setLogOnInfo(logOnInfo);
			}
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
			if (!string.IsNullOrEmpty(@event.LogOnDatasource))
				return;

			if (@event.LogOnBusinessUnitId.Equals(Guid.Empty))
			{
				if (_businessUnit != null)
				{
					var businessUnit = _businessUnit.Current();
					if (businessUnit != null)
					{
						@event.LogOnBusinessUnitId = businessUnit.Id.Value;
					}
				}
			}

			if (string.IsNullOrEmpty(@event.LogOnDatasource))
			{
				if (_dataSource != null)
				{
					@event.LogOnDatasource = _dataSource.CurrentName();
				}
			}
		}
	}
}