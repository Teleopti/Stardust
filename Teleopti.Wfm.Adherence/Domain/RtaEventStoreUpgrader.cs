using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain
{
	public class RtaEventStoreUpgrader : IRtaEventStoreUpgrader
	{
		private readonly IRtaEventStoreUpgradeWriter _writer;
		private readonly BelongsToDateMapper _mapper;

		public RtaEventStoreUpgrader(IRtaEventStoreUpgradeWriter writer, BelongsToDateMapper mapper)
		{
			_writer = writer;
			_mapper = mapper;
		}
		
		public void Upgrade()
		{
			var upgraded = 0;
			do
			{
				var events = read();
				upgraded = events.Count();
				upgrade(events);
				write(events);
			} while (upgraded > 0);
		}

		[AllBusinessUnitsUnitOfWork]
		protected virtual void upgrade(IEnumerable<UpgradeEvent> events)
		{
			events.ForEach(loadedEvent =>
			{
				var queryData = loadedEvent.Event.QueryData();
				var @event = (loadedEvent.Event as dynamic);
				@event.BelongsToDate = @event.BelongsToDate ??
									   _mapper.BelongsToDate(queryData.PersonId.Value, queryData.StartTime.Value, queryData.EndTime.Value);
			});
		}

		[UnitOfWork]
		protected virtual IEnumerable<UpgradeEvent> read() =>
			_writer.LoadForUpgrade(1, 1000);

		[UnitOfWork]
		protected virtual void write(IEnumerable<UpgradeEvent> events) =>
			events.ForEach(loadedEvent => { _writer.Upgrade(loadedEvent, 2); });
	}
}