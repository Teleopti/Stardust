using System.Linq;
using System.Text.RegularExpressions;
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

		public RtaEventStoreSynchronizer(IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
		}

		public void Synchronize()
		{
			var events = _events.LoadAll();
			var queryData = events.Select(e => e as IRtaStoredEvent).Select(x => x.QueryData());
			queryData.ForEach(q =>
			{
				var adherenceDay = _adherenceDayLoader.Load(q.PersonId.Value, q.StartTime.Value.ToDateOnly());
				var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWork != null);
				var lateForWorkText = lateForWork != null ? lateForWork.LateForWork : "0";
				var minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""));
				//Would probably be better to expose shift instead of calculating shift here 
				var shift =  new DateTimePeriod(adherenceDay.Period().StartDateTime.AddHours(1), adherenceDay.Period().EndDateTime.AddHours(-1));
				var shiftLength = (int) shift.ElapsedTime().TotalMinutes;
				
				_readModels.Upsert(new HistoricalOverviewReadModel
				{
					PersonId = q.PersonId.Value,
					Date = q.StartTime.Value.ToDateOnly(),
					Adherence = adherenceDay.Percentage(),
					WasLateForWork = lateForWork != null,
					MinutesLateForWork = minutesLateForWork,
					ShiftLength = shiftLength					
				});
			});
		}
	}
}