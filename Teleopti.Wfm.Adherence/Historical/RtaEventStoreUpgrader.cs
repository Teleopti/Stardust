using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class RtaEventStoreUpgrader : IRtaEventStoreUpgrader
	{
		private readonly IRtaEventStoreUpgradeWriter _writer;
		private readonly BelongsToDateMapper _mapper;
		private readonly IKeyValueStorePersister _keyValueStore;

		public RtaEventStoreUpgrader(IRtaEventStoreUpgradeWriter writer, BelongsToDateMapper mapper, IKeyValueStorePersister keyValueStore)
		{
			_writer = writer;
			_mapper = mapper;
			_keyValueStore = keyValueStore;
		}

		public void Upgrade()
		{
			if (IsUpgraded())
				return;

			var upgraded = 0;
			do
			{
				var events = Read();
				upgraded = events.Count();
				Upgrade(events);
				Write(events);
			} while (upgraded > 0);

			FlagUpgraded();
		}

		[ReadModelUnitOfWork]
		protected virtual bool IsUpgraded() =>
			_keyValueStore.Get("RtaEventStoreVersion", 0) == RtaEventStoreVersion.StoreVersion;

		[ReadModelUnitOfWork]
		protected virtual void FlagUpgraded() =>
			_keyValueStore.Update("RtaEventStoreVersion", RtaEventStoreVersion.StoreVersion);

		[AllBusinessUnitsUnitOfWork]
		protected virtual void Upgrade(IEnumerable<UpgradeEvent> events)
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
		protected virtual IEnumerable<UpgradeEvent> Read() =>
			_writer.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1000);

		[UnitOfWork]
		protected virtual void Write(IEnumerable<UpgradeEvent> events) =>
			events.ForEach(loadedEvent => { _writer.Upgrade(loadedEvent, 2); });
	}
}