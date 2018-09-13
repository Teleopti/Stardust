using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizer
	{
		void Synchronize();
	}


	public class NoRtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		public void Synchronize()
		{
		}
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;

		public RtaEventStoreSynchronizer(IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader, IKeyValueStorePersister keyValueStore)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
			_keyValueStore = keyValueStore;
		}
		
		[AllBusinessUnitsUnitOfWork, ReadModelUnitOfWork]
		public virtual void Synchronize()
		{
			var events = _events.LoadFrom(_keyValueStore.Get("LatestSynchronizedRTAEvent", 0));

			_keyValueStore.Update("LatestSynchronizedRTAEvent", events.MaxId.ToString());
			
			events.Events
				.Cast<IRtaStoredEvent>()
				.GroupBy(e => new { e.QueryData().PersonId, Day = e.QueryData().StartTime.Value.ToDateOnly() })
				.ForEach(group =>
				{
					var adherenceDay = _adherenceDayLoader.Load(group.Key.PersonId.Value, group.Key.Day);
					var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWork != null);
					var lateForWorkText = lateForWork != null ? lateForWork.LateForWork : "0";
					var minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""));
					var shift = new DateTimePeriod(adherenceDay.Period().StartDateTime.AddHours(1), adherenceDay.Period().EndDateTime.AddHours(-1));
					var shiftLength = (int) shift.ElapsedTime().TotalMinutes;

					_readModels.Upsert(new HistoricalOverviewReadModel
					{
						PersonId = group.Key.PersonId.Value,
						Date = group.Key.Day,
						Adherence = adherenceDay.Percentage(),
						WasLateForWork = lateForWork != null,
						MinutesLateForWork = minutesLateForWork,
						ShiftLength = shiftLength
					});
				});
		}		
	}
}