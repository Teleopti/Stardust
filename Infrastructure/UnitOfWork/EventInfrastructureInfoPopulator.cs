using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventInfrastructureInfoPopulator : IEventInfrastructureInfoPopulator
	{
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;
		private readonly INow _now;

		public static IEventInfrastructureInfoPopulator Make()
		{
			return new EventInfrastructureInfoPopulator(
				CurrentBusinessUnit.Make(),
				CurrentDataSource.Make(),
				new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()),
				new Now()
				);
		}

		public EventInfrastructureInfoPopulator(
			ICurrentBusinessUnit businessUnit, 
			ICurrentDataSource dataSource, 
			ICurrentInitiatorIdentifier initiatorIdentifier,
			INow now)
		{
			_businessUnit = businessUnit;
			_dataSource = dataSource;
			_initiatorIdentifier = initiatorIdentifier;
			_now = now;
		}

		public void PopulateEventContext(params object[] events)
		{
			foreach (var @event in events)
			{
				var timestamped = @event as ITimestamped;
				if (timestamped != null)
					setTimestamp(timestamped);
				var initiatorInfo = @event as IInitiatorContext;
				if (initiatorInfo != null)
					setInitiator(initiatorInfo);
				var logOnInfo = @event as ILogOnContext;
				if (logOnInfo != null)
					setLogOnInfo(logOnInfo);
			}
		}

		private void setTimestamp(ITimestamped @event)
		{
			if (@event.Timestamp == default(DateTime))
				@event.Timestamp = _now.UtcDateTime();
		}

		private void setInitiator(IInitiatorContext @event)
		{
			if (@event.InitiatorId != Guid.Empty)
				return;
			var initiatorIdentifier = _initiatorIdentifier.Current();
			if (initiatorIdentifier != null)
				@event.InitiatorId = initiatorIdentifier.InitiatorId;
		}

		private void setLogOnInfo(ILogOnContext @event)
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