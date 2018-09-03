using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
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


		public void syncfoo()
		{
			//truncate all
			
			var events = _events.LoadAll();
			var bigRM = new List<HistoricalOverviewReadModel>();

			var eventsForPerson = from e in events
				group e by (e as IRtaStoredEvent).QueryData().PersonId
				into eventsGroupedOnPerson
				select eventsGroupedOnPerson;
			
			eventsForPerson.ForEach(x => bigRM.AddRange(AddAllDaysForPerson(x)));

			//BatchInsert(rm);


		}

		private IEnumerable<HistoricalOverviewReadModel> AddAllDaysForPerson(IGrouping<Guid?, IEvent> grouping)
		{
			var rmPerPerson = new List<HistoricalOverviewReadModel>();

			
			grouping.ForEach(g =>
			{
				var storedEvent = g as IRtaStoredEvent;
				var typeOfEvent = g?.GetType().FullName;
				var lateForWork = typeOfEvent == typeof(PersonArrivedLateForWorkEvent).Assembly.GetName().Name;
//				var lateForWorkText = lateForWork ? string.Format(UserTexts.Resources.LateXMinutes, Math.Round(new DateTimePeriod(storedEvent.ShiftStart, storedEvent.Timestamp).ElapsedTime().TotalMinutes)) : "0";
				
				rmPerPerson.Add(new HistoricalOverviewReadModel()
				{
					PersonId = storedEvent.QueryData().PersonId.Value,
					Date = storedEvent.QueryData().StartTime.Value.ToDateOnly(),
					WasLateForWork = lateForWork
				});
				
			});

			return rmPerPerson;
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